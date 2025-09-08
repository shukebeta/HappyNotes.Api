namespace HappyNotes.Services.SyncQueue.Models;

public class TelegramSyncPayload
{
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
    public string FullContent { get; set; } = string.Empty;
    // Security: BotToken removed - will be retrieved from TelegramSettingsCacheService using UserId + ChannelId
    public string ChannelId { get; set; } = string.Empty;
    public int? MessageId { get; set; } // For UPDATE/DELETE operations
    public bool IsMarkdown { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}