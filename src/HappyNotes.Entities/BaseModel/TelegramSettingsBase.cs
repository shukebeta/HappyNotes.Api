using HappyNotes.Common.Enums;

namespace HappyNotes.Entities.BaseModel;

/// <summary>
/// Base class with common properties for Telegram settings.
/// </summary>
public abstract class TelegramSettingsBase
{
    /// <summary>
    /// Gets or sets the user identifier associated with the settings.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Gets or sets the synchronization type.
    /// </summary>
    public TelegramSyncType SyncType { get; set; }

    /// <summary>
    /// Gets or sets the synchronization value.
    /// </summary>
    public string SyncValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted token used for Telegram synchronization.
    /// </summary>
    public string EncryptedToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token remark.
    /// </summary>
    public string TokenRemark { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Telegram channel identifier used for synchronization.
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Telegram channel.
    /// </summary>
    public string ChannelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last error message.
    /// </summary>
    public string LastError { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the settings.
    /// </summary>
    public TelegramSettingStatus Status { get; set; }

    /// <summary>
    /// Gets the status text representation of the current status.
    /// </summary>
    public string StatusText => Status.StatusText();
}
