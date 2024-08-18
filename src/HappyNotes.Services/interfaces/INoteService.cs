using Api.Framework.Models;
using HappyNotes.Entities;
using HappyNotes.Models;

namespace HappyNotes.Services.interfaces;

public interface INoteService
{
    Task<long> Post(PostNoteRequest request);
    Task<Note> Get(long noteId);
    Task<bool> Delete(long id);
    Task<bool> Undelete(long id);
    Task<bool> Update(long id, PostNoteRequest request);

    Task<PageData<Note>> GetUserNotes(long userId, int pageSize, int pageNumber, bool includePrivate=false);
    /// <summary>
    /// Latest public notes / random browsing
    /// </summary>
    /// <param name="pageSize"></param>
    /// <param name="pageNumber"></param>
    /// <returns></returns>
    Task<PageData<Note>> GetPublicNotes(int pageSize, int pageNumber);

    Task<PageData<Note>> GetUserTagNotes(long userId, int pageSize, int pageNumber, string tag);

    Task<PageData<Note>> GetLinkedNotes(long userId, long noteId);
    Task<IList<Note>> Memories(string localTimezone);
    Task<IList<Note>> MemoriesOn(string localTimezone, string yyyyMMdd);
}
