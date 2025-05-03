using System.Security.Claims;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Http;

namespace HappyNotes.Services;

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
