using System.Globalization;
using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Models;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Logging;
using EventId = HappyNotes.Common.Enums.EventId;

namespace HappyNotes.Services;

public class NoteService(
    ISearchService searchService,
    IEnumerable<ISyncNoteService> syncNoteServices,
    INoteTagService noteTagService,
    INoteRepository noteRepository,
    IRepositoryBase<LongNote> longNoteRepository,
    IMapper mapper,
    ILogger<NoteService> logger
) : INoteService
{
    public async Task<long> Post(long userId, PostNoteRequest request)
    {
        var fullContent = request.Content?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(request.Content))
        {
            throw new ArgumentException("Nothing was submitted");
        }

        var note = mapper.Map<PostNoteRequest, Note>(request);
        note.UserId = userId;
        var now = DateTime.Now.ToUnixTimeSeconds();
        if (_IsPostingOrImportingOldNote(note, now)) // 5 mins
        {
            note.UpdatedAt = now;
        }

        await noteRepository.InsertAsync(note);

        if (note.IsLong)
        {
            await longNoteRepository.InsertAsync(new LongNote
            {
                Id = note.Id,
                Content = fullContent,
            });
        }

        try
        {
            await _UpdateNoteTags(note, note.TagList);
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            // ate the exception to avoid interrupting the main process
        }

        // Sync to all targets asynchronously
        foreach (var syncNoteService in syncNoteServices)
        {
            Task.Run(async () => await syncNoteService.SyncNewNote(note, fullContent));
        }
        return note.Id;
    }

    public async Task<bool> Update(long userId, long id, PostNoteRequest request)
    {
        var existingNote = await noteRepository.Get(id);
        if (existingNote == null)
        {
            throw ExceptionHelper.New(id, EventId._00100_NoteNotFound, id);
        }

        if (_NoteIsNotYours(userId, existingNote))
        {
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        var newNote = mapper.Map<PostNoteRequest, Note>(request);
        var fullContent = request.Content ?? string.Empty;

        // the following is a hack for user shukebeta only
        if (existingNote.UserId == 1 && fullContent.IsHtml())
        {
            fullContent = fullContent.ToMarkdown();
            // redo the following that automapper has done
            newNote.IsLong = fullContent.IsLong();
            newNote.Content = newNote.IsLong ? fullContent.GetShort() : fullContent;
            if (!newNote.IsMarkdown)
            {
                newNote.IsMarkdown = true;
            }
            newNote.TagList = fullContent.GetTags();
            newNote.Tags = string.Join(" ", newNote.TagList);
        }

        newNote.Id = existingNote.Id;
        newNote.UserId = existingNote.UserId;
        newNote.MastodonTootIds = existingNote.MastodonTootIds;
        newNote.TelegramMessageIds = existingNote.TelegramMessageIds;
        newNote.CreatedAt = existingNote.CreatedAt;
        newNote.UpdatedAt = DateTime.UtcNow.ToUnixTimeSeconds();
        newNote.DeletedAt = existingNote.DeletedAt;

        if (!_HasNoteChanged(existingNote, newNote, fullContent))
        {
            return true;
        }

        // update note tags first
        await _UpdateNoteTags(existingNote, newNote.TagList);

        if (newNote.IsLong)
        {
            var longNote = new LongNote { Id = id, Content = fullContent };
            await longNoteRepository.UpsertAsync(longNote, n => n.Id == id);
        }

        foreach (var syncNoteService in syncNoteServices)
        {
            Task.Run(async () => await syncNoteService.SyncEditNote(newNote, fullContent));
        }
        return await noteRepository.UpdateAsync(newNote);
    }

    private bool _HasNoteChanged(Note originalNote, Note newNote, string fullContent)
    {
        return originalNote.Content != fullContent ||
               originalNote.IsPrivate != newNote.IsPrivate ||
               originalNote.IsMarkdown != newNote.IsMarkdown ||
               originalNote.DeletedAt != newNote.DeletedAt ||
               originalNote.IsLong != newNote.IsLong ||
               originalNote.Tags != newNote.Tags ||
               originalNote.MastodonTootIds != newNote.MastodonTootIds ||
               originalNote.TelegramMessageIds != newNote.TelegramMessageIds;
    }

    public async Task<PageData<Note>> GetUserNotes(long userId, int pageSize, int pageNumber,
        bool includePrivate = false)
    {
        return await noteRepository.GetUserNotes(userId, pageSize, pageNumber, includePrivate);
    }

    public async Task<PageData<Note>> GetPublicNotes(int pageSize, int pageNumber)
    {
        if (pageNumber > Constants.PublicNotesMaxPage)
        {
            throw new Exception(
                $"We only provide at most {Constants.PublicNotesMaxPage} page of public notes at the moment");
        }

        var notes = await noteRepository.GetPublicNotes(pageSize, pageNumber);
        if (notes.TotalCount > Constants.PublicNotesMaxPage * pageSize)
        {
            notes.TotalCount = Constants.PublicNotesMaxPage * pageSize;
        }

        return notes;
    }

    public async Task<PageData<Note>> GetUserTagNotes(long userId, int pageSize, int pageNumber, string tag)
    {
        return await noteRepository.GetUserTagNotes(userId, tag, pageSize, pageNumber);
    }

    public async Task<PageData<Note>> GetUserKeywordNotes(long userId, int pageSize, int pageNumber, string keyword, NoteFilterType filter = NoteFilterType.Normal)
    {
        var (noteIds, total) = await searchService.GetNoteIdsByKeywordAsync(userId, keyword, pageNumber, pageSize, filter);
        var notes = await _GetNotesByIds(noteIds);
        return new PageData<Note>()
        {
            DataList = notes,
            PageIndex = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
        };
    }

    public async Task<PageData<Note>> GetLinkedNotes(long userId, long noteId)
    {
        return await noteRepository.GetLinkedNotes(userId, noteId);
    }

    /// <summary>
    /// Providing memories like
    /// - 5 years ago, [what happened today]
    /// - 4 years ago...
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="localTimezone"></param>
    /// <returns></returns>
    public async Task<IList<Note>> Memories(long userId, string localTimezone)
    {
        var earliest = await noteRepository.GetFirstOrDefaultAsync(w => w.UserId == userId, "CreatedAt");
        if (earliest == null) return [];
        var notes = new List<Note>();
        var periodList = _GetTimestamps(earliest.CreatedAt, localTimezone);
        foreach (var start in periodList)
        {
            var note = await noteRepository.GetFirstOrDefaultAsync(w =>
                w.UserId == userId && w.CreatedAt >= start && w.CreatedAt < start + 86400 &&
                w.DeletedAt == null, "CreatedAt ASC");
            if (note != null)
            {
                notes.Add(note);
            }
        }

        return notes;
    }

    public async Task<IList<Note>> MemoriesOn(long userId, string localTimezone, string yyyyMMdd)
    {
        // Get the specified time zone
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimezone);
        // Parse the date string to a DateTime object
        DateTimeOffset dateTimeOffset;
        try
        {
            dateTimeOffset = DateTimeOffset.ParseExact(yyyyMMdd, "yyyyMMdd", CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            throw new ArgumentException($"Invalid date format '{yyyyMMdd}'. Expected format: yyyyMMdd (e.g., 20250212)");
        }
        var dayStartTimestamp = dateTimeOffset.GetDayStartTimestamp(timeZone);
        var notes = await noteRepository.GetListAsync(w =>
            w.UserId == userId && w.CreatedAt >= dayStartTimestamp &&
            w.CreatedAt < dayStartTimestamp + 86400 && w.DeletedAt == null, w => w.CreatedAt);
        return notes;
    }

    public async Task<Note> Get(long userId, long noteId, bool includeDeleted = false)
    {
        var note = await noteRepository.Get(noteId);
        if (note is null)
        {
            throw ExceptionHelper.New(noteId, EventId._00100_NoteNotFound, noteId);
        }

        if (note.IsPrivate && _NoteIsNotYours(userId, note))
        {
            throw ExceptionHelper.New(noteId, EventId._00101_NoteIsPrivate, noteId);
        }

        if (!includeDeleted && note.DeletedAt.HasValue)
        {
            throw ExceptionHelper.New(noteId, EventId._00104_NoteIsDeleted, noteId);
        }

        return note;
    }

    public async Task<bool> Delete(long userId, long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note == null)
        {
            throw new Exception($"Note with Id: {id} does not exist");
        }

        if (_NoteIsNotYours(userId, note))
        {
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        // already deleted before
        if (note.DeletedAt is not null)
        {
            return true;
        }

        note.DeletedAt = DateTime.UtcNow.ToUnixTimeSeconds();
        foreach (var syncNoteService in syncNoteServices)
        {
            Task.Run(async () => await syncNoteService.SyncDeleteNote(note));
        }
        return await noteRepository.UpdateAsync(note);
    }

    public async Task<bool> Undelete(long userId, long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(n => n.Id == id); // Fetches regardless of DeletedAt

        if (note is null)
        {
            // If the note doesn't exist at all.
            throw ExceptionHelper.New(id, EventId._00100_NoteNotFound, id);
        }

        if (_NoteIsNotYours(userId, note))
        {
            // Check ownership before attempting undelete.
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        if (note.DeletedAt is null)
        {
            // we don't need to throw an exception in this case
            return true;
        }

        // Now, attempt the undelete operation via the repository.
        await noteRepository.UndeleteAsync(id);

        // Assuming success if no exception was thrown.
        return true;
    }

    private static bool _IsPostingOrImportingOldNote(Note note, long currentTimestamp)
    {
        return currentTimestamp - note.CreatedAt > 300;
    }

    private static bool _NoteIsNotYours(long userId, Note note)
    {
        return userId != note.UserId;
    }

    private async Task _UpdateNoteTags(Note note, List<string> newTags)
    {
        if (newTags.Count != 0)
        {
            await noteTagService.Upsert(note, newTags);
        }
        await noteTagService.RemoveUnusedTags(note.Id, newTags);
    }

    internal static long[] _GetTimestamps(long initialUnixTimestamp, string timeZoneId)
    {
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var startDate = initialUnixTimestamp.ToDateTimeOffset(targetTimeZone);
        var today = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, targetTimeZone);

        var timestamps = new List<long>();

        // Add yearly anniversaries (same month/day in previous years)
        timestamps.AddRange(_GetYearlyAnniversaryTimestamps(startDate, today, targetTimeZone));

        // Add periodic milestones (6 months, 3 months, 1 month ago)
        // Only include if the target month actually has the same day
        var periodicOffsets = new[] { -6, -3, -1 };
        foreach (var monthsOffset in periodicOffsets)
        {
            if (_TryGetExactMonthOffset(today, monthsOffset, out var targetDate) &&
                _IsWithinUserHistoryPeriod(targetDate, startDate))
            {
                timestamps.Add(targetDate.GetDayStartTimestamp(targetTimeZone));
            }
        }

        // Always add yesterday and today
        timestamps.Add(today.AddDays(-1).GetDayStartTimestamp(targetTimeZone));
        timestamps.Add(today.GetDayStartTimestamp(targetTimeZone));

        return timestamps.ToArray();
    }

    private static IEnumerable<long> _GetYearlyAnniversaryTimestamps(DateTimeOffset startDate, DateTimeOffset today, TimeZoneInfo timeZone)
    {
        if (startDate.Year >= today.Year) yield break;

        var anniversaryMonth = today.Month;
        var anniversaryDay = today.Day;

        // Determine the first anniversary year
        var firstAnniversaryYear = startDate.Month <= anniversaryMonth && startDate.Day <= anniversaryDay
            ? startDate.Year
            : startDate.Year + 1;

        for (var year = firstAnniversaryYear; year < today.Year; year++)
        {
            if (_TryCreateDate(year, anniversaryMonth, anniversaryDay, timeZone, out var timestamp))
            {
                yield return timestamp;
            }
        }
    }

    internal static bool _TryCreateDate(int year, int month, int day, TimeZoneInfo timeZone, out long timestamp)
    {
        timestamp = 0;
        try
        {
            var targetDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
            var date = new DateTimeOffset(targetDate, timeZone.GetUtcOffset(targetDate));
            timestamp = date.ToUnixTimeSeconds();
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            // Invalid date (e.g., Feb 30)
            return false;
        }
    }

    /// <summary>
    /// Attempts to calculate a date that is exactly the specified number of months offset from the given date.
    /// Returns false if the target month doesn't have the same day (e.g., Feb 31 doesn't exist).
    /// </summary>
    internal static bool _TryGetExactMonthOffset(DateTimeOffset date, int monthsOffset, out DateTimeOffset targetDate)
    {
        targetDate = default;

        // Use AddMonths to handle year rollover automatically
        var tempDate = date.AddMonths(monthsOffset);

        // Check if the day stayed the same (meaning the target month has this day)
        if (tempDate.Day != date.Day)
        {
            return false; // Target month doesn't have this day
        }

        targetDate = tempDate;
        return true;
    }

    /// <summary>
    /// Checks if a target date falls within the user's history period (not before they started using the app).
    /// </summary>
    private static bool _IsWithinUserHistoryPeriod(DateTimeOffset targetDate, DateTimeOffset userStartDate)
    {
        return targetDate >= userStartDate;
    }

    public async Task<PageData<Note>> GetUserDeletedNotes(long userId, int pageSize, int pageNumber)
    {
        return await noteRepository.GetUserDeletedNotes(userId, pageSize, pageNumber);
    }

    public async Task PurgeUserDeletedNotes(long userId)
    {
        await noteRepository.PurgeUserDeletedNotes(userId);
    }

    /// <summary>
    /// Retrieves notes by their IDs.
    /// </summary>
    /// <param name="noteIds">A list of note IDs.</param>
    /// <returns>A list of notes corresponding to the provided IDs.</returns>
    private async Task<IList<Note>> _GetNotesByIds(IList<long> noteIds)
    {
        if (!noteIds.Any())
        {
            return [];
        }

        return await noteRepository.GetListByIdsAsync(noteIds.ToArray());
    }
}
