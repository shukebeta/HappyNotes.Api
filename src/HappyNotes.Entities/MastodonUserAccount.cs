using Api.Framework.Helper;
using HappyNotes.Common.Enums;
using SqlSugar;

namespace HappyNotes.Entities;

public class MastodonUserAccount
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long UserId { get; set; }
    public string InstanceUrl { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;

    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;

    public MastodonUserAccountStatus Status { get; set; }

    /// <summary>
    /// Gets the status text representation of the current status.
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string StatusText => Status.StatusText();

    public long CreatedAt { get; set; }

    private string? _decryptedAccessToken;
    public string DecryptedAccessToken(string key)
    {
        return _decryptedAccessToken ??= TextEncryptionHelper.Decrypt(AccessToken, key);
    }
}
