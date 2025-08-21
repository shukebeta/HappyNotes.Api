using HappyNotes.Common.Enums;
using HappyNotes.Entities.BaseModel;
using SqlSugar;

namespace HappyNotes.Entities;

/// <summary>
/// Represents the settings for Telegram synchronization.
/// </summary>
public class TelegramSettings : TelegramSettingsBase
{
    /// <summary>
    /// Gets or sets the unique identifier for the settings.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp indicating when the settings were created.
    /// </summary>
    public long CreatedAt { get; set; }

    /// <summary>
    /// Gets the status text representation of the current status.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public new string StatusText => Status.StatusText();
}
