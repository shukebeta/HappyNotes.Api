using HappyNotes.Entities;
using HappyNotes.Models;

namespace HappyNotes.Services.interfaces;

public interface INoteService
{
    Task<long> Post(PostNoteRequest request);
    Task<Note?> Get(long id);
    Task<bool> Delete(long id);
    Task<bool> Undelete(long id);
    Task<bool> Update(long id, PostNoteRequest request);
}
