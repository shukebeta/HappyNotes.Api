using SqlSugar;

namespace HappyNotes.Entities;

public class MastodonApplications
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    public string Name { get; set; }
    public string Website { get; set; }
    public string ApplicationId { get; set; }
    public string InstanceUrl { get; set; }
    public string RedirectUri { get; set; }
    public string Scopes { get; set; }
    public int MaxTootChars { get; set; } = 500;
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
}
