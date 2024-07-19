namespace HappyNotes.Services.interfaces;

public interface INoteTagService
{
    Task Upsert(long noteId, List<string> tags);
    Task Delete(long noteId, List<string> tags);
}
