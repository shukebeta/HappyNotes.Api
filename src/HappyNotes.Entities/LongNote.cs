using SqlSugar;

namespace HappyNotes.Entities;

public class LongNote
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public string Content { get; set; } = string.Empty;
}