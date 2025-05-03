namespace HappyNotes.Services.interfaces;

public interface IAccountService
{
        Task<bool> IsValidLogin(string username, string password);
        Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword);
}
