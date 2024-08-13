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
                await noteTagRepository.InsertAsync(new NoteTag()
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
        return await noteTagRepository.db.Queryable<NoteTag>().Where(w => w.UserId == userId).
            GroupBy(g => g.Tag).
            Select(s => new TagCount()
            {
                Tag = s.Tag,
                Count = SqlFunc.AggregateCount(s.Tag)
            }).
            OrderByDescending(o => o.Count).
            Take(limit).
            ToListAsync();
    }
}
