using HappyNotes.Common;
using HappyNotes.Common.Enums;

namespace HappyNotes.Entities;

public class TelegramSettings
{
    public long UserId { get; set; }
    public TelegramSyncType SyncType { get; set; }
    public string SyncValue { get; set; } = string.Empty;
    public string EncryptedToken { get; set; } = string.Empty;
    public string TokenRemark { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public TelegramSettingStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public long CreatedAt { get; set; }
}
