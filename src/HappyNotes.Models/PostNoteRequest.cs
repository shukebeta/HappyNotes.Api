using HappyNotes.Common;

namespace HappyNotes.Models;

public class PostNoteRequest
{
    private string? _content;

    public string? Content
    {
        get => _content;
        set => _content = value?.NormalizeNewlines();
    }
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
