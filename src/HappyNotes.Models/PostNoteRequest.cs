namespace HappyNotes.Models;

public class PostNoteRequest
{
    public string? Content { get; set; }
    public string[]? Attachments { get; set; }
    public bool IsPrivate { get; set; } = true;
}
