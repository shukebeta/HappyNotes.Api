namespace HappyNotes.Common.Enums;

public enum MastodonUserAccountStatus
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
    Disabled = Normal | Inactive,
}
