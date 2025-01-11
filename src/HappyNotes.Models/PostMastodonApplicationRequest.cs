namespace HappyNotes.Models;

public class PostMastodonApplicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public int ApplicationId { get; set; } = 0;
    public string InstanceUrl { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
