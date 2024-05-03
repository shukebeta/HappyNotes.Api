namespace HappyNotes.Models;

public class RegisterRequest: LoginRequest
{
    public string Email { get; set; } = string.Empty;
}