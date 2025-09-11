namespace HappyNotes.Services.SyncQueue.Models;

public class ManticoreSearchSyncPayload
{
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, UNDELETE
    public string FullContent { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
