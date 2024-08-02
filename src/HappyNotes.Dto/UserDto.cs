namespace HappyNotes.Dto;

public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Gravatar { get; set; } = string.Empty;
    public long CreatedAt { get; set; }
}
