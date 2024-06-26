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

namespace HappyNotes.Services;

public class NoteService(
    INoteRepository noteRepository,
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

        Note note = new Note
        {
            UserId = currentUser.Id,
            IsPrivate = request.IsPrivate,
            CreateAt = DateTime.UtcNow.ToUnixTimeSeconds(),
            IsLong = false,
        };

        if (fullContent.IsLong())
        {
            note.IsLong = true;
            note.Content = fullContent.GetShort();
            await noteRepository.InsertAsync(note);
            await longNoteRepository.InsertAsync(new LongNote
            {
                Id = note.Id,
                Content = fullContent
            });
            return note.Id;
        }

        note.Content = fullContent;
        await noteRepository.InsertAsync(note);
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

        if (fullContent.IsLong())
        {
            note.Content = fullContent.GetShort();
            note.IsPrivate = request.IsPrivate;
            note.UpdateAt = DateTime.UtcNow.ToUnixTimeSeconds();
            note.IsLong = true;
            await noteRepository.UpdateAsync(note);
            var longNote = new LongNote
            {
                Id = id,
                Content = fullContent
            };
            if (await longNoteRepository.GetFirstOrDefaultAsync(x => x.Id == id) is null)
            {
                return await longNoteRepository.InsertAsync(longNote);
            }

            return await longNoteRepository.UpdateAsync(longNote);
        }

        note.Content = fullContent;
        note.IsPrivate = request.IsPrivate;
        note.UpdateAt = DateTime.UtcNow.ToUnixTimeSeconds();
        note.IsLong = false;
        return await noteRepository.UpdateAsync(note);
    }

    public async Task<PageData<Note>> MyLatest(int pageSize, int pageNumber)
    {
        return await noteRepository.GetUserNotes(currentUser.Id, pageSize, pageNumber, true);
    }

    public async Task<PageData<Note>> Latest(int pageSize, int pageNumber)
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
                w.UserId.Equals(currentUser.Id) && w.CreateAt >= start && w.CreateAt < start + 86400);
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
            w.CreateAt < dayStartTimestamp + 86400);
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

        if (startYear > currentYear)
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

        if (currentMonth - startMonth >= 1)
        {
            for (var month = startDay <= currentDay
                     ? startMonth
                     : startMonth + 1;
                 month < currentMonth;
                 month++)
            {
                try
                {
                    var targetDate = new DateTimeOffset(currentYear, month, currentDay, 0, 0, 0,
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

        if (currentDay - startDay >= 28)
        {
            // 4 weeks ago
            timestamps.Add(todayStartTimeOffset.AddDays(-28).ToUnixTimeSeconds());
        }

        if (currentDay - startDay >= 21)
        {
            // 3 weeks
            timestamps.Add(todayStartTimeOffset.AddMonths(-21).ToUnixTimeSeconds());
        }

        if (currentDay - startDay >= 14)
        {
            // 2 weeks ago
            timestamps.Add(todayStartTimeOffset.AddDays(-14).ToUnixTimeSeconds());
        }

        if (currentDay - startDay >= 7)
        {
            // 1 week ago
            timestamps.Add(todayStartTimeOffset.AddMonths(-7).ToUnixTimeSeconds());
        }


        timestamps.Add(todayStartTimeOffset.AddDays(-5).ToUnixTimeSeconds());
        timestamps.Add(todayStartTimeOffset.AddDays(-3).ToUnixTimeSeconds());
        timestamps.Add(todayStartTimeOffset.AddDays(-2).ToUnixTimeSeconds());
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
        if (note.DeleteAt != null)
        {
            return true;
        }

        note.DeleteAt = DateTime.UtcNow.ToUnixTimeSeconds();
        return await noteRepository.UpdateAsync(note);
    }

    public async Task<bool> Undelete(long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note == null)
        {
            throw ExceptionHelper.New(id, EventId._00100_NoteNotFound, id);
        }

        if (note.DeleteAt == null)
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
