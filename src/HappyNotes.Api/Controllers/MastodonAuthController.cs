using Api.Framework;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
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
    public ApiResult SetState([FromHeader(Name = "X-State")] string state)
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
    public async Task<ApiResult> Callback(string instanceUrl, string code, string state)
    {
        logger.LogInformation("Starting Mastodon OAuth callback for instance: {InstanceUrl}, state: {State}",
            instanceUrl, state);

        try
        {
            if (string.IsNullOrEmpty(state))
            {
                logger.LogError("Empty state received in OAuth callback for {InstanceUrl}", instanceUrl);
                throw new Exception("Unexpected Empty state received");
            }

            // Validate state if needed (to prevent CSRF attacks)
            var userId = generalMemoryCacheService.Get<long>(state);
            if (userId == 0)
            {
                logger.LogError("Invalid state {State} received for {InstanceUrl}", state, instanceUrl);
                throw new Exception("Invalid state");
            }

            logger.LogDebug("OAuth callback for user {UserId}, instance: {InstanceUrl}", userId, instanceUrl);

            var application = await mastodonApplicationRepository.GetFirstOrDefaultAsync(a => a.InstanceUrl == instanceUrl);
            if (application == null)
            {
                logger.LogError("No application found for instanceUrl: {InstanceUrl}", instanceUrl);
                throw new Exception($"There is No application for instanceUrl: {instanceUrl}");
            }

            logger.LogDebug("Found application for {InstanceUrl}, exchanging code for token", instanceUrl);

            // Exchange the code for an access token
            var clientId = TextEncryptionHelper.Decrypt(application.ClientId, _jwtConfig.SymmetricSecurityKey);
            var clientSecret = TextEncryptionHelper.Decrypt(application.ClientSecret, _jwtConfig.SymmetricSecurityKey);
            var response =
                await ExchangeCodeForToken(userId, instanceUrl, clientId, clientSecret, code,
                    application.RedirectUri);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Successfully authenticated user {UserId} with {InstanceUrl}", userId, instanceUrl);
                return Success("Successfully authenticated with Mastodon. You can close this page.");
            }

            // Get the status code
            var statusCode = response.StatusCode;
            // Get the error message
            var errorMessage = await response.Content.ReadAsStringAsync();

            logger.LogError("Failed to authenticate user {UserId} with {InstanceUrl}. Status: {StatusCode}, Error: {ErrorMessage}",
                userId, instanceUrl, statusCode, errorMessage);

            return Fail($"Failed to authenticate with Mastodon: {statusCode}:{errorMessage}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OAuth callback failed for instance: {InstanceUrl}. Error: {ErrorMessage}",
                instanceUrl, ex.Message);
            return Fail($"Authentication failed: {ex.Message}");
        }
    }

    private async Task<HttpResponseMessage> ExchangeCodeForToken(long userId, string instanceUrl, string clientId,
        string clientSecret, string code, string redirectUri)
    {
        logger.LogInformation("Exchanging OAuth code for token: user {UserId}, instance {InstanceUrl}",
            userId, instanceUrl);

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30); // Set timeout for external API call

            var tokenRequest = new Dictionary<string, string>
            {
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"grant_type", "authorization_code"},
                {"code", code},
                {"redirect_uri", redirectUri},
            };

            logger.LogDebug("Sending token request to {InstanceUrl}/oauth/token for user {UserId}",
                instanceUrl, userId);

            var response = await client.PostAsync($"{instanceUrl}/oauth/token", new FormUrlEncodedContent(tokenRequest));

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                logger.LogDebug("Received successful token response from {InstanceUrl} for user {UserId}",
                    instanceUrl, userId);

                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);
                if (tokenResponse != null)
                {
                    logger.LogDebug("Creating MastodonUserAccount for user {UserId}, instance {InstanceUrl}",
                        userId, instanceUrl);

                    var mastodonUserAccount = new MastodonUserAccount
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

                    logger.LogInformation("Successfully created MastodonUserAccount for user {UserId}, instance {InstanceUrl}",
                        userId, instanceUrl);
                }
                else
                {
                    logger.LogError("Failed to deserialize token response from {InstanceUrl} for user {UserId}. Response: {JsonResponse}",
                        instanceUrl, userId, jsonResponse);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Token exchange failed for {InstanceUrl}, user {UserId}. Status: {StatusCode}, Response: {ErrorContent}",
                    instanceUrl, userId, response.StatusCode, errorContent);
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during token exchange for {InstanceUrl}, user {UserId}: {ErrorMessage}",
                instanceUrl, userId, ex.Message);
            throw;
        }
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
