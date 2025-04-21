namespace HappyNotes.Dto;

public class NoteDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public bool IsLong { get; set; }
    public bool IsMarkdown { get; set; }
    public bool IsPrivate { get; set; }

    public long CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
    public long? DeletedAt { get; set; }

    public UserDto User { get; set; } = new();
}
