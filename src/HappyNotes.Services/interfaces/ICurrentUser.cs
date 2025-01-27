namespace HappyNotes.Services.interfaces;

public interface ICurrentUser
{
    long Id { get; }
    string Username { get; }
    string Email { get; }
}
