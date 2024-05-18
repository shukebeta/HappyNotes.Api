namespace HappyNotes.Models;

public class CurrentUser
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    /// <summary>
    /// A unix timestamp that indicates when the token will expire
    /// </summary>
    public long TokenValidTo { get; set; }
}
