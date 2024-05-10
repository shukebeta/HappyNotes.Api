namespace HappyNotes.Dto;

public class NoteDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int FavoriteCount { get; set; }
    public int Status { get; set; }
    public bool IsLong { get; set; }
    public bool IsPrivate { get; set; }
    public int CreateAt { get; set; }
    public int UpdateAt { get; set; }
}
