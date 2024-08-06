using HappyNotes.Entities.BaseModel;

namespace HappyNotes.Dto;

/// <summary>
/// Data transfer object for Telegram settings.
/// </summary>
public class TelegramSettingsDto : TelegramSettingsBase
{
    /// <summary>
    /// Gets or sets the unique identifier for the settings.
    /// </summary>
    public long Id { get; set; }
}
