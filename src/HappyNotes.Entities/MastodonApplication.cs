using SqlSugar;

namespace HappyNotes.Entities;

public class MastodonApplication
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Website { get; init; } = string.Empty;
    public int ApplicationId { get; init; }
    public string InstanceUrl { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = string.Empty;
    public string Scopes { get; init; } = string.Empty;
    public int MaxTootChars { get; set; } = 500;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
}
