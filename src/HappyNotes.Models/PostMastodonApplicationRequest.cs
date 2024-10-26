namespace HappyNotes.Models;

public class PostMastodonApplicationRequest
{
    public string Name { get; set; }
    public string Website { get; set; }
    public int ApplicationId { get; set; }
    public string InstanceUrl { get; set; }
    public string RedirectUri { get; set; }
    public string Scopes { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
