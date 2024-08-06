namespace HappyNotes.Common.Enums;

[Flags]
public enum TelegramSettingStatus
{
    /// <summary>
    /// The initial status.
    /// </summary>
    Created = 1,

    /// <summary>
    /// Successfully synced at least one message.
    /// </summary>
    Tested = 2,

    /// <summary>
    /// Temporarily deactivated.
    /// </summary>
    Inactive = 4,

    /// <summary>
    /// Account has been banned or sync failed.
    /// </summary>
    Error = 8,

    /// <summary>
    /// Alias for normal setting (Created and Tested).
    /// </summary>
    Normal = Created | Tested,

    /// <summary>
    /// Alias for disabled setting (Created, Tested, and Inactive).
    /// </summary>
    Disabled = Created | Tested | Inactive,
}

public static class TelegramSettingStatusExtensions
{
    /// <summary>
    /// Gets the human-friendly status text for the given TelegramSettingStatus.
    /// </summary>
    /// <param name="status">The status to convert to text.</param>
    /// <returns>A string representing the human-friendly status text.</returns>
    public static string StatusText(this TelegramSettingStatus status)
    {
        switch (status)
        {
            case TelegramSettingStatus.Created:
            case TelegramSettingStatus.Tested:
            case TelegramSettingStatus.Normal:
            case TelegramSettingStatus.Disabled:
                return status.ToString();
            default:
                var statuses = new List<string>();

                if (status.HasFlag(TelegramSettingStatus.Created))
                    statuses.Add("Created");

                if (status.HasFlag(TelegramSettingStatus.Tested))
                    statuses.Add("Tested");

                if (status.HasFlag(TelegramSettingStatus.Inactive))
                    statuses.Add("Inactive");

                if (status.HasFlag(TelegramSettingStatus.Error))
                    statuses.Add("Error");

                return statuses.Count > 0 ? string.Join(", ", statuses) : $"Unknown Status: {(int)status}";
        }
    }
}
