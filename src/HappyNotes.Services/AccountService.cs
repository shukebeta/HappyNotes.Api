using Api.Framework;
using Api.Framework.Exceptions;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Services.interfaces;

namespace HappyNotes.Services;

public class AccountService(IRepositoryBase<User> userRepository): IAccountService
{
    public async Task<bool> IsValidLogin(string username, string password)
    {
        var user = await userRepository.GetFirstOrDefaultAsync(x => x.Username == username);
        return user != null && user.Password.Equals(CommonHelper.CalculateSha256Hash(password + user.Salt));
    }

    public async Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
    {
        var user = await userRepository.GetFirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            throw ExceptionHelper.New(EventId._00107_UserNotFound, userId);
        }

        // Validate current password
        if (!user.Password.Equals(CommonHelper.CalculateSha256Hash(currentPassword + user.Salt)))
        {
            throw ExceptionHelper.New(EventId._00108_OldPasswordIncorrect);
        }

        var (newSalt, newPasswordHash) = CommonHelper.GetSaltedPassword(newPassword);
        var changedRows = await userRepository.UpdateAsync(_ => new User
        {
            Salt = newSalt,
            Password = newPasswordHash,
            UpdatedAt = DateTime.UtcNow.ToUnixTimeSeconds(),
        }, where => where.Id == userId);
        return changedRows == 1;
    }
}
