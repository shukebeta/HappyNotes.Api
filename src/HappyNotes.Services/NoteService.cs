using System.Linq.Expressions;
using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Result;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;

namespace HappyNotes.Services;

public class NoteService(
    IRepositoryBase<Note> noteRepository,
    IRepositoryBase<LongNote> longNoteRepository,
    CurrentUser currentUser) : INoteService
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
            throw new Exception($"Note with Id: {id} does not exist");
        }

        var fullContent = request.Content?.Trim() ?? string.Empty;

        if (fullContent.IsLong())
        {
            note.Content = fullContent.GetShort();
            note.IsPrivate = request.IsPrivate;
            note.UpdateAt = DateTime.UtcNow.ToUnixTimestamp();
            note.IsLong = false;
            await noteRepository.UpdateAsync(note);
            return await longNoteRepository.UpdateAsync(new LongNote()
            {
                Id = id,
                Content = fullContent
            });
        }

        note.Content = fullContent;
        note.IsPrivate = request.IsPrivate;
        note.UpdateAt = DateTime.UtcNow.ToUnixTimestamp();
        note.IsLong = false;
        return await noteRepository.UpdateAsync(note);
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
