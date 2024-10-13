using SqlSugar;

namespace HappyNotes.Entities;

public class MastodonSyncStatus
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    public long NoteId { get; set; }
    public long UserId { get; set; }
    public int ApplicationId { get; set; }
    public string TootId { get; set; }

    public int SyncStatus { get; set; }
    public string ErrorMessage { get; set; }

    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
    public long LastSyncAttempt { get; set; }
}
