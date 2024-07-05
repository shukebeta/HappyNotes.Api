namespace HappyNotes.Services.interfaces;

public interface INoteTagService
{
    Task Upsert(long noteId, string[] tags);
    Task Delete(long noteId, string[] tags);
}
