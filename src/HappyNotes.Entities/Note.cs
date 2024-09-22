using Api.Framework;
using HappyNotes.Common;
using HappyNotes.Models;
using SqlSugar;

namespace HappyNotes.Entities;

public class Note: EntityBase
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int FavoriteCount { get; set; } = 0;
    public bool IsLong { get; set; }
    public bool IsMarkdown { get; set; }
    public bool IsPrivate { get; set; }
    public string Tags { get; set; }
    public string? TelegramMessageIds { get; set; }

    [SugarColumn(IsIgnore = true)] public User User { get; set; } = default!;
    [SugarColumn(IsIgnore = true)] public List<string> TagList { get; set; }

    public void UpdateTelegramMessageIds(List<SyncedChannel> channels)
    {
        TelegramMessageIds = string.Join(",", channels.Select(r => $"{r.ChannelId}:{r.MessageId}"));
    }
}
