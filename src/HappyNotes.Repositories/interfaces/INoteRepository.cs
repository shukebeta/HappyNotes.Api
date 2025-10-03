using Api.Framework;
using Api.Framework.Models;
using HappyNotes.Entities;

namespace HappyNotes.Repositories.interfaces;

public interface INoteRepository : IRepositoryBase<Note>
{
    Task<Note> Get(long noteId);

    Task<PageData<Note>> GetUserTagNotes(long userId, string tag, int pageSize, int pageNumber, bool isAsc = false);
    Task<PageData<Note>> GetUserNotes(long userId, int pageSize, int pageNumber, bool includePrivate = false,
        bool isAsc = false);
    Task<PageData<Note>> GetPublicNotes(int pageSize, int pageNumber, bool isAsc = false, long? excludeUserId = null);
    Task<PageData<Note>> GetLinkedNotes(long userId, long noteId, int max = 100);

    Task<PageData<Note>> GetUserDeletedNotes(long userId, int pageSize, int pageNumber);
    Task PurgeUserDeletedNotes(long userId);
    Task UndeleteAsync(long noteId);
}
