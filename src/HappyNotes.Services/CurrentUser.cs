using HappyNotes.Services.interfaces;

namespace HappyNotes.Services;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public long Id => string.IsNullOrEmpty(GetClaimValue(ClaimTypes.NameIdentifier)) ? 0 : long.Parse(GetClaimValue(ClaimTypes.NameIdentifier));
    public string Username => GetClaimValue(ClaimTypes.Name);
    public string Email => GetClaimValue(ClaimTypes.Email);

    private string GetClaimValue(string claimType)
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(claimType) ?? string.Empty;
    }
}
