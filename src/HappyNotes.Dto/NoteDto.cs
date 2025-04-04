namespace HappyNotes.Dto;

public class NoteDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public int FavoriteCount { get; set; }
    public bool IsLong { get; set; }
    public bool IsMarkdown { get; set; }
    public bool IsPrivate { get; set; }
    public int CreatedAt { get; set; }
    public int UpdatedAt { get; set; }
    public int DeletedAt { get; set; }

    public UserDto User { get; set; } = new();
}
