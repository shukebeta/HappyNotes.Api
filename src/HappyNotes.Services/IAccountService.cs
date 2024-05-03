namespace HappyNotes.Services;

public interface IAccountService
{
        Task<bool> IsValidLogin(string username, string password);
}