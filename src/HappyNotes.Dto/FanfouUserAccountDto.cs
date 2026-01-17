using HappyNotes.Common.Enums;

namespace HappyNotes.Dto;

/// <summary>
/// Data transfer object for Fanfou user account.
/// </summary>
public class FanfouUserAccountDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public FanfouSyncType SyncType { get; set; }
    public FanfouUserAccountStatus Status { get; set; }
    public long CreatedAt { get; set; }
    public string StatusText => Status.ToString();
}

/// <summary>
/// Request model for adding/updating Fanfou account
/// </summary>
public class PostFanfouAccountRequest
{
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public FanfouSyncType SyncType { get; set; } = FanfouSyncType.All;
}

/// <summary>
/// Request model for updating Fanfou account sync type
/// </summary>
public class PutFanfouAccountRequest
{
    public FanfouSyncType SyncType { get; set; }
}

/// <summary>
/// Model for synced Fanfou account display
/// </summary>
public class FanfouSyncedAccount
{
    public long UserAccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string StatusId { get; set; } = string.Empty;
}
