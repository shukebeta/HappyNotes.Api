namespace HappyNotes.Models;

public class MastodonSyncedInstance
{
    public long UserAccountId { get; set; }
    public string TootId { get; set; } = string.Empty;
}
