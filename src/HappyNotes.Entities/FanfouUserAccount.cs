using Api.Framework.Helper;
using HappyNotes.Common.Enums;
using SqlSugar;

namespace HappyNotes.Entities;

public class FanfouUserAccount
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    // OAuth credentials (encrypted)
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string AccessTokenSecret { get; set; } = string.Empty;

    public FanfouSyncType SyncType { get; set; } = FanfouSyncType.All;
    public FanfouUserAccountStatus Status { get; set; }

    public long CreatedAt { get; set; }

    private string? _decryptedAccessToken;
    private string? _decryptedTokenSecret;

    public string DecryptedAccessToken(string key)
    {
        return _decryptedAccessToken ??= TextEncryptionHelper.Decrypt(AccessToken, key);
    }

    public string DecryptedAccessTokenSecret(string key)
    {
        return _decryptedTokenSecret ??= TextEncryptionHelper.Decrypt(AccessTokenSecret, key);
    }
}
