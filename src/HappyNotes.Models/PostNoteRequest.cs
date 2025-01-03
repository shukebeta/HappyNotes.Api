namespace HappyNotes.Models;

public class PostNoteRequest
{
    public string? Content { get; set; }
    public string[]? Attachments { get; set; }
    public bool IsPrivate { get; set; } = true;
    public bool IsMarkdown { get; set; }

    /// <summary>
    /// in yyyy-MM-dd hh:mm:ss format
    /// </summary>
    public string? PublishDateTime { get; set; }
    /// <summary>
    /// Such as 'Pacific/Auckland'
    /// </summary>
    public string? TimezoneId { get; set; }
}
