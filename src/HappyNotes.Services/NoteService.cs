using System.Globalization;
using System.Linq.Expressions;
using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Models;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
using WeihanLi.Extensions;

namespace HappyNotes.Services;

public class NoteService(
    IMapper mapper,
    IRepositoryBase<Note> noteRepository,
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

        Note note = new Note()
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
            await longNoteRepository.InsertAsync(new LongNote()
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

        var fullContent = request.Content?.Trim() ?? string.Empty;

        if (fullContent.IsLong())
        {
            note.Content = fullContent.GetShort();
            note.IsPrivate = request.IsPrivate;
            note.UpdateAt = DateTime.UtcNow.ToUnixTimeSeconds();
            note.IsLong = true;
            await noteRepository.UpdateAsync(note);
            var longNote = new LongNote()
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

    public async Task<PageData<NoteDto>> MyLatest(int pageSize, int pageNumber)
    {
        var notes = await _GetNotesByPage(currentUser.Id, pageSize, pageNumber);
        return mapper.Map<PageData<NoteDto>>(notes);
    }

    public async Task<PageData<NoteDto>> Latest(int pageSize, int pageNumber)
    {
        if (pageNumber > Constants.PublicNotesMaxPage)
        {
            throw new Exception(
                $"We only provide at most {Constants.PublicNotesMaxPage} page of public notes at the moment");
        }

        Expression<Func<Note, bool>> where = n => !n.IsPrivate && n.DeleteAt == null;
        var notes = await noteRepository.GetPageListAsync(pageNumber, pageSize, where, n => n.CreateAt, false);
        return mapper.Map<PageData<NoteDto>>(notes);
    }

    /// <summary>
    /// Providing memories like
    /// - 5 years ago, [what happened today]
    /// - 4 years ago...
    /// </summary>
    /// <param name="localTimezone"></param>
    /// <returns></returns>
    public async Task<List<NoteDto>> Memories(string localTimezone)
    {
        var period = UnixTimestampHelper.GetDayUnixTimestamps(localTimezone);
        var earliest = await noteRepository.GetFirstOrDefaultAsync(w => w.UserId.Equals(currentUser.Id));
        if (earliest == null) return [];
        var notes = new List<Note>();
        var periodList = _GetDayStartList(period.EndUnixTimestamp, earliest);
        foreach (var start in periodList)
        {
            var note = await noteRepository.GetFirstOrDefaultAsync(w =>
                w.UserId.Equals(currentUser.Id) && w.CreateAt >= start && w.CreateAt < start + 86400);
            if (note != null)
            {
                notes.Add(note);
            }
        }

        return mapper.Map<List<NoteDto>>(notes);
    }

    public async Task<List<NoteDto>> MemoriesIn(string localTimezone, string yyyyMMdd)
    {
        var dateTimeOffset = UnixTimestampHelper.GetDateTimeOffset(yyyyMMdd, "yyyyMMdd", localTimezone);
        // Create a DateTimeOffset object with the parsed date and the local time zone offset
        var dayStart = dateTimeOffset.ToUnixTimeSeconds();
        var notes = await noteRepository.GetListAsync(w =>
            w.UserId.Equals(currentUser.Id) && w.CreateAt >= dayStart &&
            w.CreateAt < dayStart + 86400);
        return mapper.Map<List<NoteDto>>(notes);
    }

    private static List<long> _GetDayStartList(long endOfToday, Note earliest)
    {
        var periodList = new List<long>();
        var offset = endOfToday - earliest.CreateAt;

        const int oneDay = 86400;
        const int oneYear = 365 * oneDay;
        const int halfYear = 180 * oneDay;
        const int threeMonths = 90 * oneDay;
        const int twoMonths = 60 * oneDay;
        const int oneMonths = 30 * oneDay;
        const int fourWeeks = 28 * oneDay;
        const int threeWeeks = 21 * oneDay;
        const int twoWeeks = 14 * oneDay;
        const int oneWeek = 7 * oneDay;
        if (offset >= oneYear) // a year
        {
            var n = offset / oneYear;
            while (n > 0)
            {
                periodList.Add(endOfToday - n * oneYear);
                offset -= oneYear;
                n -= 1;
            }

            switch (n)
            {
                case 1:
                    periodList.Add(endOfToday - halfYear);
                    periodList.Add(endOfToday - threeMonths);
                    periodList.Add(endOfToday - oneMonths);
                    break;
                case 2:
                    periodList.Add(endOfToday - halfYear);
                    periodList.Add(endOfToday - threeMonths);
                    break;
                case 3:
                    periodList.Add(endOfToday - halfYear);
                    break;
            }
        }
        else if (offset >= halfYear) // half a year
        {
            periodList.Add(endOfToday - halfYear);
            periodList.Add(endOfToday - threeMonths);
            periodList.Add(endOfToday - twoMonths);
            periodList.Add(endOfToday - oneMonths);
        }
        else if (offset >= threeMonths)
        {
            periodList.Add(endOfToday - threeMonths);
            periodList.Add(endOfToday - twoMonths);
            periodList.Add(endOfToday - oneMonths);
            periodList.Add(endOfToday - threeWeeks);
        }
        else if (offset >= fourWeeks)
        {
            periodList.Add(endOfToday - fourWeeks);
            periodList.Add(endOfToday - threeWeeks);
            periodList.Add(endOfToday - twoWeeks);
            periodList.Add(endOfToday - oneWeek);
        }
        else if (offset >= oneWeek)
        {
            periodList.Add(endOfToday - oneWeek);
            periodList.Add(endOfToday - 5 * oneDay);
            periodList.Add(endOfToday - 3 * oneDay);
        }

        return periodList;
    }

    private async Task<PageData<Note>> _GetNotesByPage(long userId, int pageSize, int pageNumber, bool isAsc = false,
        NoteType noteType = NoteType.All)
    {
        Expression<Func<Note, bool>> where = n => n.UserId.Equals(userId) && n.DeleteAt == null;
        if (noteType != NoteType.All)
        {
            where = where.And(n => n.IsPrivate.Equals(noteType == NoteType.Private));
        }

        var notes = await noteRepository.GetPageListAsync(pageNumber, pageSize, where, n => n.CreateAt, isAsc);
        return notes;
    }

    public async Task<Note?> Get(long noteId)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == noteId);
        if (note is null)
        {
            throw ExceptionHelper.New(noteId, EventId._00100_NoteNotFound, noteId);
        }

        if (note.IsPrivate && _IsNotAuthor(note))
        {
            throw ExceptionHelper.New(noteId, EventId._00101_NoteIsPrivate, noteId);
        }

        if (note is not {IsLong: true,}) return note;
        var longNote = await longNoteRepository.GetFirstOrDefaultAsync(x => x.Id == noteId);

        if (!string.IsNullOrWhiteSpace(longNote?.Content))
        {
            note.Content = longNote.Content;
        }

        return note;
    }

    private bool _IsNotAuthor(Note note)
    {
        return currentUser is null || currentUser.Id != note.UserId;
    }

    public async Task<bool> Delete(long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note == null)
        {
            throw new Exception($"Note with Id: {id} does not exist");
        }

        if (_IsNotAuthor(note))
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

        if (_IsNotAuthor(note))
        {
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        note.DeleteAt = null;
        return await noteRepository.UpdateAsync(note);
    }
}
