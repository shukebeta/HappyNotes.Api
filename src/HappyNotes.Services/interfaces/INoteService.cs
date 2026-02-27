using Api.Framework.Models;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Models;

namespace HappyNotes.Services.interfaces;

public interface INoteService
{
    Task<long> Post(long userId, PostNoteRequest request);
    Task<Note> Get(long userId, long noteId);
    Task<bool> Delete(long userId, long id);
    Task<bool> Undelete(long userId, long id);
    Task<bool> Update(long userId, long id, PostNoteRequest request);
    Task<bool> SetIsPrivate(long userId, long id, bool isPrivate);

    Task<PageData<Note>> GetUserNotes(long userId, int pageSize, int pageNumber, bool includePrivate = false);
    /// <summary>
    /// Latest public notes / random browsing
    /// If excludeUserId is provided, notes from that user will be excluded (useful to avoid showing caller's own public notes).
    /// </summary>
    /// <param name="pageSize"></param>
    /// <param name="pageNumber"></param>
    /// <param name="excludeUserId">Optional user id to exclude from results</param>
    /// <returns></returns>
    Task<PageData<Note>> GetPublicNotes(int pageSize, int pageNumber, long? excludeUserId = null);

    Task<PageData<Note>> GetUserTagNotes(long userId, int pageSize, int pageNumber, string tag);
    Task<PageData<Note>> GetUserKeywordNotes(long userId, int pageSize, int pageNumber, string keyword, NoteFilterType filter = NoteFilterType.Normal);
    Task<PageData<Note>> SearchUserNotes(long userId, int pageSize, int pageNumber, string keyword, NoteFilterType filter = NoteFilterType.Normal);

    Task<PageData<Note>> GetLinkedNotes(long userId, long noteId);
    Task<IList<Note>> Memories(long userId, string localTimezone);
    Task<IList<Note>> MemoriesOn(long userId, string localTimezone, string yyyyMMdd);

    Task<PageData<Note>> GetUserDeletedNotes(long userId, int pageSize, int pageNumber);
    Task PurgeUserDeletedNotes(long userId);
}
