namespace HappyNotes.Services.SyncQueue.Models;

public class MastodonSyncPayload
{
    public string InstanceUrl { get; set; } = string.Empty;
    public long UserAccountId { get; set; }
    public string FullContent { get; set; } = string.Empty;
    public string? TootId { get; set; } // For UPDATE/DELETE operations
    public bool IsPrivate { get; set; }
    public bool IsMarkdown { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
