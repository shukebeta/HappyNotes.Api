using Api.Framework;
using SqlSugar;

namespace HappyNotes.Entities;

public class Note: EntityBase
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int FavoriteCount { get; set; } = 0;
    public int Status { get; set; } = 1;
    public bool IsLong { get; set; }
    public bool IsPrivate { get; set; }
}
