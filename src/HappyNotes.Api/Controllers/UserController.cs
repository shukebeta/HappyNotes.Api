using Api.Framework;
using HappyNotes.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

public class UserController(IRepositoryBase<User> userRepository)
    : BaseController
{
    [HttpGet]
    public async Task<User> Get(long userId)
    {
        var user = await userRepository.GetFirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            throw new Exception("User not found");
        }

        return user;
    }
}