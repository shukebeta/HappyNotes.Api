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
public static class MastodonUserAccountStatusExtensions
{
    /// <summary>
    /// Gets the human-friendly status text for the given MastodonUserAccountStatus.
    /// </summary>
    /// <param name="status">The status to convert to text.</param>
    /// <returns>A string representing the human-friendly status text.</returns>
    public static string StatusText(this MastodonUserAccountStatus status)
    {
        switch (status)
        {
            case MastodonUserAccountStatus.Created:
            case MastodonUserAccountStatus.Tested:
            case MastodonUserAccountStatus.Normal:
            case MastodonUserAccountStatus.Disabled:
                return status.ToString();
            default:
                var statuses = new List<string>();

                if (status.HasFlag(MastodonUserAccountStatus.Created))
                    statuses.Add("Created");

                if (status.HasFlag(MastodonUserAccountStatus.Tested))
                    statuses.Add("Tested");

                if (status.HasFlag(MastodonUserAccountStatus.Inactive))
                    statuses.Add("Inactive");

                if (status.HasFlag(MastodonUserAccountStatus.Error))
                    statuses.Add("Error");

                return statuses.Count > 0 ? string.Join(", ", statuses) : $"Unknown Status: {(int)status}";
        }
    }
}
