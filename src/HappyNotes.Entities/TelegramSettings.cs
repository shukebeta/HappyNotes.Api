using HappyNotes.Common;

namespace HappyNotes.Entities;

public class TelegramSettings
{
    public long UserId { get; set; }
    public TelegramSyncType SyncType { get; set; }
    public string SyncValue { get; set; } = string.Empty;
    public string EncryptedTelegramToken { get; set; } = string.Empty;
    public string TokenRemark { get; set; } = string.Empty;
    public string TelegramChannelId { get; set; } = string.Empty;
    public long CreateAt { get; set; }
}
