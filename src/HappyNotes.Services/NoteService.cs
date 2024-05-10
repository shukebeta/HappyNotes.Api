using System.Linq.Expressions;
using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
using WeihanLi.Extensions;

namespace HappyNotes.Services;

public class NoteService(IMapper mapper
    , IRepositoryBase<Note> noteRepository
    , IRepositoryBase<LongNote> longNoteRepository
    , CurrentUser currentUser
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
            CreateAt = DateTime.UtcNow.ToUnixTimestamp(),
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
            note.UpdateAt = DateTime.UtcNow.ToUnixTimestamp();
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
        note.UpdateAt = DateTime.UtcNow.ToUnixTimestamp();
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
            throw new Exception($"We only provide at most {Constants.PublicNotesMaxPage} page of public notes at the moment");
        }
        Expression<Func<Note, bool>> where = n => !n.IsPrivate;
        var notes = await noteRepository.GetPageListAsync(pageNumber, pageSize, where, n => n.CreateAt, false);
        return mapper.Map<PageData<NoteDto>>(notes);
    }

    private async Task<PageData<Note>> _GetNotesByPage(long userId, int pageSize, int pageNumber, bool isAsc=false, NoteType noteType=NoteType.All)
    {
        Expression<Func<Note, bool>> where = n => n.UserId.Equals(userId);
        if (noteType != NoteType.All)
        {
            where = where.And(n => n.IsPrivate.Equals(noteType == NoteType.Private));
        }
        var notes = await noteRepository.GetPageListAsync(pageNumber, pageSize, where, n => n.CreateAt, isAsc);
        return notes;
    }

    public async Task<Note?> Get(long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note is not {IsLong: true,}) return note;
        var longNote = await longNoteRepository.GetFirstOrDefaultAsync(x => x.Id == id);

        if (!string.IsNullOrWhiteSpace(longNote?.Content))
        {
            note.Content = longNote.Content;
        }

        return note;
    }

    public async Task<bool> Delete(long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note == null)
        {
            throw new Exception($"Note with Id: {id} does not exist");
        }

        if (note.DeleteAt != null)
        {
            return true;
        }

        note.DeleteAt = DateTime.UtcNow.ToUnixTimestamp();
        return await noteRepository.UpdateAsync(note);
    }

    public async Task<bool> Undelete(long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note == null)
        {
            throw new Exception($"Note with Id: {id} does not exist");
        }

        if (note.DeleteAt != null)
        {
            throw new Exception($"Note with Id: {id} was not deleted at all");
        }

        note.DeleteAt = null;
        return await noteRepository.UpdateAsync(note);
    }
}
