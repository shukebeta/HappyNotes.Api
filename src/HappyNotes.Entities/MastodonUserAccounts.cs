using SqlSugar;

namespace HappyNotes.Entities;

public class MastodonUserAccounts
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long UserId { get; set; }
    public int ApplicationId { get; set; }
    public string Scope { get; set; }

    public string MastodonUserId { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string AvatarUrl { get; set; }

    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string TokenType { get; set; }

    public long CreatedAt { get; set; }
    public long ExpiresAt { get; set; }
    public long UpdatedAt { get; set; }
}
