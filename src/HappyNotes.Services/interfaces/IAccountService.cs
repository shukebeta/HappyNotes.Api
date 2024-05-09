namespace HappyNotes.Services.interfaces;

public interface IAccountService
{
        Task<bool> IsValidLogin(string username, string password);
}
