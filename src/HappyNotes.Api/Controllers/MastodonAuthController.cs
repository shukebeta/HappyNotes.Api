using Api.Framework;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Services;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyNotes.Api.Controllers;

[Authorize]
public class MastodonAuthController(
    IRepositoryBase<MastodonApplication> mastodonApplicationRepository,
    IRepositoryBase<MastodonUserAccount> mastodonUserAccountRepository,
    ICurrentUser currentUser,
    ILogger<MastodonAuthController> logger,
    IOptions<JwtConfig> jwtConfig,
    IGeneralMemoryCacheService generalMemoryCacheService) : BaseController
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;

    [HttpPost]
    public ApiResult<bool> SetState([FromHeader(Name = "X-State")] string state)
    {
        if (string.IsNullOrEmpty(state))
        {
            return Fail("State cannot be empty");
        }

        // Optionally decode the header if it's Base64 encoded
        // string decodedState = Encoding.UTF8.GetString(Convert.FromBase64String(state));

        var cacheId = state;
        generalMemoryCacheService.Set(cacheId, currentUser.Id);

        logger.LogInformation("State set for user {UserId}", currentUser.Id);

        return Success();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ApiResult<bool>> Callback(string instanceUrl, string code, string state)
    {
        if (string.IsNullOrEmpty(state)) throw new Exception("Unexpected Empty state received");
        // Validate state if needed (to prevent CSRF attacks)
        var userId = generalMemoryCacheService.Get<long>(state);
        if (userId == 0) throw new Exception("Invalid state");

        var application = await mastodonApplicationRepository.GetFirstOrDefaultAsync(a => a.InstanceUrl == instanceUrl);
        if (application == null) throw new Exception($"There is No application for instanceUrl: {instanceUrl}");
        // Exchange the code for an access token
        var clientId = TextEncryptionHelper.Decrypt(application.ClientId, _jwtConfig.SymmetricSecurityKey);
        var clientSecret = TextEncryptionHelper.Decrypt(application.ClientSecret, _jwtConfig.SymmetricSecurityKey);
        var response =
            await ExchangeCodeForToken(userId, instanceUrl, clientId, clientSecret, code,
                application.RedirectUri);

        if (response.IsSuccessStatusCode)
        {
            return Success("Successfully authenticated with Mastodon. You can close this page.");
        }

        // Get the status code
        var statusCode = response.StatusCode;
        // Get the error message
        var errorMessage = await response.Content.ReadAsStringAsync();

        return Fail($"Failed to authenticate with Mastodon: {statusCode}:{errorMessage}");
    }

    private async Task<HttpResponseMessage> ExchangeCodeForToken(long userId, string instanceUrl, string clientId,
        string clientSecret, string code, string redirectUri)
    {
        var client = new HttpClient();
        var tokenRequest = new Dictionary<string, string>
        {
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"grant_type", "authorization_code"},
            {"code", code},
            {"redirect_uri", redirectUri},
        };

        var response = await client.PostAsync($"{instanceUrl}/oauth/token", new FormUrlEncodedContent(tokenRequest));

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);
            if (tokenResponse != null)
            {
                var mastodonUserAccount = new MastodonUserAccount()
                {
                    UserId = userId,
                    InstanceUrl = instanceUrl,
                    AccessToken =
                        TextEncryptionHelper.Encrypt(tokenResponse.access_token, _jwtConfig.SymmetricSecurityKey),
                    TokenType = tokenResponse.token_type,
                    Scope = tokenResponse.scope,
                    Status = MastodonUserAccountStatus.Normal,
                    SyncType = MastodonSyncType.All,
                    CreatedAt = long.Parse(tokenResponse.created_at),
                };
                await mastodonUserAccountRepository.InsertAsync(mastodonUserAccount);
            }
        }
        return response;
    }
}

public class TokenResponse
{
    // ReSharper disable once InconsistentNaming
    public string access_token { get; set; } = string.Empty;

    // ReSharper disable once InconsistentNaming
    public string token_type { get; set; } = string.Empty;

    // ReSharper disable once InconsistentNaming
    public string scope { get; set; } = string.Empty;

    // ReSharper disable once InconsistentNaming
    public string created_at { get; set; } = string.Empty;
}
