using SqlSugar;

namespace HappyNotes.Entities;

public class NoteTag
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    public long NoteId { get; set; }
    public string Tag { get; set; }
    public long CreatedAt { get; set; }
}
