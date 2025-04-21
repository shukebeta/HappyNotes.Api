using HappyNotes.Entities;
using HappyNotes.Models;

namespace HappyNotes.Services.interfaces;

public interface INoteTagService
{
    Task Upsert(Note note, List<string> tags);
    Task Delete(long noteId, List<string> tags);
    Task RemoveUnusedTags(long noteId, List<string> toKeepTags);
    Task<List<TagCount>> GetTagData(long userId, int limit = 50);
}
