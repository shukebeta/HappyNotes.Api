using Api.Framework;
using Api.Framework.Helper;
using HappyNotes.Entities;

namespace HappyNotes.Services;

public class AccountService(IRepositoryBase<User> userRepository): IAccountService
{
    public async Task<bool> IsValidLogin(string username, string password)
    {
        var user = await userRepository.GetFirstOrDefaultAsync(x => x.Username == username);
        return user != null && user.Password.Equals(CommonHelper.CalculateSha256Hash(password + user.Salt));
    }
}