namespace HappyNotes.Common;

/// <summary>
/// Defines the types of note synchronization for Telegram.
/// </summary>
public enum TelegramSyncType
{
    /// <summary>
    /// Sync only public notes (IsPrivate = false).
    /// </summary>
    Public = 1,

    /// <summary>
    /// Sync only private notes (IsPrivate = true).
    /// </summary>
    Private = 2,

    /// <summary>
    /// Sync all notes.
    /// </summary>
    All = 3,

    /// <summary>
    /// Sync notes with a specific tag.
    /// </summary>
    Tag = 4,
}
