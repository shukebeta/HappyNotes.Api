namespace HappyNotes.Common.Enums;

public enum MastodonSyncType
{
    /// <summary>
    /// Sync all notes
    /// </summary>
    All = 1,

    /// <summary>
    /// Public notes nonly
    /// </summary>
    PublicOnly = 2,

    /// <summary>
    /// Sync notes that have a mastodon tag
    /// </summary>
    TagMastodonOnly = 3,
}

public static class MastodonSyncTypeExtensions
{
    public static MastodonSyncType Next(this MastodonSyncType syncType)
    {
        return syncType switch
        {
            MastodonSyncType.All => MastodonSyncType.PublicOnly,
            MastodonSyncType.PublicOnly => MastodonSyncType.TagMastodonOnly,
            MastodonSyncType.TagMastodonOnly => MastodonSyncType.All,
            _ => MastodonSyncType.All,
        };

    }
}
