using Api.Framework;
using HappyNotes.Entities;
using HappyNotes.Services.interfaces;

namespace HappyNotes.Services;

public class NoteTagService(
    IRepositoryBase<NoteTag> noteTagRepository
) : INoteTagService
{
    public async Task Upsert(long noteId, string[] tags)
    {
        foreach (var tag in tags)
        {
            if (await noteTagRepository.GetFirstOrDefaultAsync(t =>
                    t.NoteId == noteId && t.TagName.Equals(tag.ToLower())) == null)
                await noteTagRepository.InsertAsync(new NoteTag()
                {
                    NoteId = noteId,
                    TagName = tag.ToLower(),
                    CreateAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                });
        }
    }

    public async Task Delete(long noteId, string[] tags)
    {
        foreach (var tag in tags)
        {
            await noteTagRepository.DeleteAsync(t => t.NoteId == noteId && t.TagName.Equals(tag.ToLower()));
        }
    }
}
