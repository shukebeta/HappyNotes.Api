using System.Globalization;
using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Models;
using HappyNotes.Common;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Logging;
using EventId = HappyNotes.Common.EventId;

namespace HappyNotes.Services;

public class NoteService(
    ILogger<NoteService> logger,
    INoteRepository noteRepository,
    INoteTagService noteTagService,
    IRepositoryBase<LongNote> longNoteRepository,
    CurrentUser currentUser
) : INoteService
{
    public async Task<long> Post(PostNoteRequest request)
    {
        var fullContent = request.Content?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(fullContent) && (request.Attachments == null || !request.Attachments.Any()))
        {
            throw new ArgumentException("Nothing was submitted");
        }

        var note = new Note
        {
            UserId = currentUser.Id,
            IsPrivate = request.IsPrivate,
            IsMarkdown = request.IsMarkdown,
            CreateAt = DateTime.UtcNow.ToUnixTimeSeconds(),
            IsLong = fullContent.IsLong(),
            Tags = string.Join(' ', fullContent.GetTags()),
            Content = fullContent.IsLong() ? fullContent.GetShort() : fullContent
        };

        await noteRepository.InsertAsync(note);

        if (note.IsLong)
        {
            await longNoteRepository.InsertAsync(new LongNote
            {
                Id = note.Id,
                Content = fullContent
            });
        }

        try
        {
            await _UpdateNoteTags(note, fullContent);
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            // ate the exception to avoid interrupting the main process
        }

        return note.Id;
    }


    public async Task<bool> Update(long id, PostNoteRequest request)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note == null)
        {
            throw ExceptionHelper.New(id, EventId._00100_NoteNotFound, id);
        }

        if (_NoteIsNotYours(note))
        {
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        var fullContent = request.Content?.Trim() ?? string.Empty;
        // we first update tags
        await _UpdateNoteTags(note, fullContent);

        note.IsPrivate = request.IsPrivate;
        note.IsMarkdown = request.IsMarkdown;
        note.UpdateAt = DateTime.UtcNow.ToUnixTimeSeconds();
        note.IsLong = fullContent.IsLong();
        note.Tags = string.Join(' ', fullContent.GetTags());

        if (note.IsLong)
        {
            var longNote = new LongNote
            {
                Id = id,
                Content = fullContent
            };
            await longNoteRepository.UpsertAsync(longNote, n => n.Id == id);
        }
        note.Content = fullContent.GetShort();
        return await noteRepository.UpdateAsync(note);
    }

    private async Task _UpdateNoteTags(Note note, string fullContent)
    {
        var oldTags = (string.IsNullOrWhiteSpace(note.Tags)) ? [] : note.Tags.Split(" ").ToList();
        var tags = fullContent.GetTags();
        if (oldTags.Any())
        {
            var toDelete = oldTags.Except(tags).ToList();
            if (toDelete.Any()) await noteTagService.Delete(note.Id, toDelete);
        }

        if (tags.Any())
        {
            await noteTagService.Upsert(note.Id, tags);
        }
    }

    public async Task<PageData<Note>> GetUserNotes(long userId, int pageSize, int pageNumber, bool includePrivate=false)
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

    public async Task<PageData<Note>> GetUserTagNotes(long userId, int pageSize, int pageNumber, string tag, bool includePrivate=false)
    {
        return await noteRepository.GetUserTagNotes(userId, tag, pageSize, pageNumber, includePrivate);
    }

    public async Task<PageData<Note>> GetPublicTagNotes(int pageSize, int pageNumber, string tag)
    {
        return await noteRepository.GetPublicTagNotes(tag, pageSize, pageNumber);
    }

    /// <summary>
    /// Providing memories like
    /// - 5 years ago, [what happened today]
    /// - 4 years ago...
    /// </summary>
    /// <param name="localTimezone"></param>
    /// <returns></returns>
    public async Task<IList<Note>> Memories(string localTimezone)
    {
        var earliest = await noteRepository.GetFirstOrDefaultAsync(w => w.UserId.Equals(currentUser.Id));
        if (earliest == null) return [];
        var notes = new List<Note>();
        var periodList = _GetTimestamps(earliest.CreateAt, localTimezone);
        foreach (var start in periodList)
        {
            var note = await noteRepository.GetFirstOrDefaultAsync(w =>
                w.UserId.Equals(currentUser.Id) && w.CreateAt >= start && w.CreateAt < start + 86400 && w.DeleteAt == null);
            if (note != null)
            {
                notes.Add(note);
            }
        }

        return notes;
    }

    public async Task<IList<Note>> MemoriesOn(string localTimezone, string yyyyMMdd)
    {
        // Get the specified time zone
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimezone);
        // Parse the date string to a DateTime object
        var dayStartTimestamp = DateTimeOffset.ParseExact(yyyyMMdd, "yyyyMMdd", CultureInfo.InvariantCulture)
            .GetDayStartTimestamp(timeZone);
        var notes = await noteRepository.GetListAsync(w =>
            w.UserId.Equals(currentUser.Id) && w.CreateAt >= dayStartTimestamp &&
            w.CreateAt < dayStartTimestamp + 86400 && w.DeleteAt == null);
        return (notes);
    }

    private static long[] _GetTimestamps(long initialUnixTimestamp, string timeZoneId)
    {
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var startDateTimeOffset = initialUnixTimestamp.ToDateTimeOffset(targetTimeZone);

        var startYear = startDateTimeOffset.Year;
        var startMonth = startDateTimeOffset.Month;
        var startDay = startDateTimeOffset.Day;

        var nowInTargetTimeZone = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, targetTimeZone);
        var todayStartTimeOffset = new DateTimeOffset(nowInTargetTimeZone.Year, nowInTargetTimeZone.Month,
            nowInTargetTimeZone.Day, 0, 0, 0, nowInTargetTimeZone.Offset);

        var currentYear = nowInTargetTimeZone.Year;
        var currentMonth = nowInTargetTimeZone.Month;
        var currentDay = nowInTargetTimeZone.Day;

        var timestamps = new List<long>();

        if (startYear < currentYear)
        {
            for (var year = startMonth <= currentMonth && startDay <= currentDay
                     ? startYear
                     : startYear + 1;
                 year < currentYear;
                 year++)
            {
                try
                {
                    var targetDate = new DateTimeOffset(year, currentMonth, currentDay, 0, 0, 0,
                        nowInTargetTimeZone.Offset);
                    timestamps.Add(targetDate.ToUnixTimeSeconds());
                }
                catch (ArgumentOutOfRangeException)
                {
                    // skip invalid dates，such as 02-30, 02-31
                    // do nothing
                }
            }
        }

        if (startDateTimeOffset.AddMonths(6).EarlierThan(nowInTargetTimeZone))
        {
            timestamps.Add(todayStartTimeOffset.AddMonths(-6).ToUnixTimeSeconds());
        }

        if (startDateTimeOffset.AddMonths(3).EarlierThan(nowInTargetTimeZone))
        {
            timestamps.Add(todayStartTimeOffset.AddMonths(-3).ToUnixTimeSeconds());
        }

        if (startDateTimeOffset.AddMonths(1).EarlierThan(nowInTargetTimeZone))
        {
            timestamps.Add(todayStartTimeOffset.AddMonths(-1).ToUnixTimeSeconds());
        }

        timestamps.Add(todayStartTimeOffset.AddDays(-1).ToUnixTimeSeconds());
        timestamps.Add(todayStartTimeOffset.ToUnixTimeSeconds());
        return timestamps.ToArray();
    }

    public async Task<Note> Get(long noteId)
    {
        var note = await noteRepository.Get(noteId);
        if (note is null)
        {
            throw ExceptionHelper.New(noteId, EventId._00100_NoteNotFound, noteId);
        }

        if (note.IsPrivate && _NoteIsNotYours(note))
        {
            throw ExceptionHelper.New(noteId, EventId._00101_NoteIsPrivate, noteId);
        }

        return note;
    }

    private bool _NoteIsNotYours(Note note)
    {
        return currentUser.Id != note.UserId;
    }

    public async Task<bool> Delete(long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note == null)
        {
            throw new Exception($"Note with Id: {id} does not exist");
        }

        if (_NoteIsNotYours(note))
        {
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        // already deleted before
        if (note.DeleteAt is not null)
        {
            return true;
        }

        note.DeleteAt = DateTime.UtcNow.ToUnixTimeSeconds();
        return await noteRepository.UpdateAsync(note);
    }

    public async Task<bool> Undelete(long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note is null)
        {
            throw ExceptionHelper.New(id, EventId._00100_NoteNotFound, id);
        }

        if (note.DeleteAt is null)
        {
            throw ExceptionHelper.New(id, EventId._00103_NoteIsNotDeleted, id);
        }

        if (_NoteIsNotYours(note))
        {
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        note.DeleteAt = null;
        return await noteRepository.UpdateAsync(note);
    }
}
