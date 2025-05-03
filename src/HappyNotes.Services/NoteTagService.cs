using Api.Framework;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
using SqlSugar;

namespace HappyNotes.Services;

public class NoteTagService(
    IRepositoryBase<NoteTag> noteTagRepository
) : INoteTagService
{
    public async Task Upsert(Note note, List<string> tags)
    {
        foreach (var tag in tags)
        {
            if (await noteTagRepository.GetFirstOrDefaultAsync(t =>
                    t.NoteId == note.Id && t.Tag.Equals(tag.ToLower())) == null)
                await noteTagRepository.InsertAsync(new NoteTag
                {
                    NoteId = note.Id,
                    UserId = note.UserId,
                    Tag = tag.ToLower(),
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                });
        }
    }

    public async Task Delete(long noteId, List<string> tags)
    {
        foreach (var tag in tags)
        {
            await noteTagRepository.DeleteAsync(t => t.NoteId == noteId && t.Tag.Equals(tag.ToLower()));
        }
    }

    public async Task<List<TagCount>> GetTagData(long userId, int limit = 50)
    {
        return await noteTagRepository.db.Queryable<NoteTag>().InnerJoin<Note>((nt, n) => nt.NoteId == n.Id)
            .Where((nt, n) => nt.UserId == userId && n.DeletedAt == null)
            .GroupBy(nt => nt.Tag)
            .Select(nt => new TagCount
            {
                Tag = nt.Tag,
                Count = SqlFunc.AggregateCount(nt.Tag)
            })
            .MergeTable()
            .OrderByDescending(t => t.Count).
            Take(limit).
            ToListAsync();
    }

    public async Task RemoveUnusedTags(long noteId, List<string> toKeepTags)
    {
        await noteTagRepository.DeleteAsync(t => t.NoteId == noteId && !toKeepTags.Contains(t.Tag));
    }
}
