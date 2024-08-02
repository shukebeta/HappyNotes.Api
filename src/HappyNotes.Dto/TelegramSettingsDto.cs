using HappyNotes.Common;

namespace HappyNotes.Dto;

public class TelegramSettingsDto
{
    public long UserId { get; set; }
    public TelegramSyncType SyncType { get; set; }
    public string SyncValue { get; set; } = string.Empty;
    public string EncryptedToken { get; set; } = string.Empty;
    public string TokenRemark { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public TelegramSettingStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
}
