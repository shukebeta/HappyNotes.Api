using System.Web;
using Api.Framework;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using HappyNotes.Entities;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HappyNotes.Api.Controllers;

[Authorize]
public class FanfouAuthController(
    IRepositoryBase<FanfouUserAccount> fanfouUserAccountRepository,
    ICurrentUser currentUser,
    ILogger<FanfouAuthController> logger,
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

        var cacheId = state;
        generalMemoryCacheService.Set(cacheId, currentUser.Id);

        logger.LogInformation("State set for user {UserId}", currentUser.Id);

        return Success();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ApiResult> Callback(string oauth_token, string oauth_verifier, string state)
    {
        logger.LogInformation("Starting Fanfou OAuth callback. State: {State}", state);

        try
        {
            if (string.IsNullOrEmpty(state))
            {
                logger.LogError("Empty state received in OAuth callback");
                throw new Exception("Unexpected empty state received");
            }

            // Validate state
            var userId = generalMemoryCacheService.Get<long>(state);
            if (userId == 0)
            {
                logger.LogError("Invalid state {State} received", state);
                throw new Exception("Invalid state");
            }

            logger.LogDebug("OAuth callback for user {UserId}", userId);

            // Get request token and secret from cache (stored during request-token phase)
            var requestTokenKey = $"fanfou_request_token_{state}";
            var requestTokenData = generalMemoryCacheService.Get<Dictionary<string, string>>(requestTokenKey);
            if (requestTokenData == null)
            {
                logger.LogError("No request token data found for state {State}", state);
                throw new Exception("No request token data found");
            }

            var requestToken = requestTokenData["oauth_token"];
            var requestTokenSecret = requestTokenData["oauth_token_secret"];
            var consumerKey = requestTokenData["consumer_key"];
            var consumerSecret = TextEncryptionHelper.Decrypt(requestTokenData["consumer_secret"], _jwtConfig.SymmetricSecurityKey);

            // Exchange request token for access token
            var (accessToken, accessTokenSecret, username) = await ExchangeRequestTokenForAccessToken(
                consumerKey, consumerSecret, requestToken, requestTokenSecret, oauth_verifier);

            if (string.IsNullOrEmpty(accessToken))
            {
                return Fail("Failed to obtain access token from Fanfou");
            }

            // Check if account already exists
            var existingAccount = await fanfouUserAccountRepository.GetFirstOrDefaultAsync(
                a => a.UserId == userId && a.Username == username);

            if (existingAccount != null)
            {
                // Update existing account
                existingAccount.AccessToken = TextEncryptionHelper.Encrypt(accessToken, _jwtConfig.SymmetricSecurityKey);
                existingAccount.AccessTokenSecret = TextEncryptionHelper.Encrypt(accessTokenSecret, _jwtConfig.SymmetricSecurityKey);
                existingAccount.Status = Common.Enums.FanfouUserAccountStatus.Active;
                await fanfouUserAccountRepository.UpdateAsync(existingAccount);

                logger.LogInformation("Updated existing Fanfou user account for user {UserId}, username {Username}",
                    userId, username);
            }
            else
            {
                // Create new account
                var fanfouUserAccount = new FanfouUserAccount
                {
                    UserId = userId,
                    Username = username,
                    ConsumerKey = TextEncryptionHelper.Encrypt(consumerKey, _jwtConfig.SymmetricSecurityKey),
                    ConsumerSecret = TextEncryptionHelper.Encrypt(consumerSecret, _jwtConfig.SymmetricSecurityKey),
                    AccessToken = TextEncryptionHelper.Encrypt(accessToken, _jwtConfig.SymmetricSecurityKey),
                    AccessTokenSecret = TextEncryptionHelper.Encrypt(accessTokenSecret, _jwtConfig.SymmetricSecurityKey),
                    SyncType = Common.Enums.FanfouSyncType.All,
                    Status = Common.Enums.FanfouUserAccountStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                };
                await fanfouUserAccountRepository.InsertAsync(fanfouUserAccount);

                logger.LogInformation("Created new Fanfou user account for user {UserId}, username {Username}",
                    userId, username);
            }

            logger.LogInformation("Successfully authenticated user {UserId} with Fanfou as {Username}",
                userId, username);

            return Success("Successfully authenticated with Fanfou. You can close this page.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OAuth callback failed. Error: {ErrorMessage}", ex.Message);
            return Fail($"Authentication failed: {ex.Message}");
        }
    }

    private async Task<(string accessToken, string accessTokenSecret, string username)> ExchangeRequestTokenForAccessToken(
        string consumerKey,
        string consumerSecret,
        string requestToken,
        string requestTokenSecret,
        string oauthVerifier)
    {
        logger.LogInformation("Exchanging request token for access token");

        try
        {
            // For now, we'll need to use the FanfouStatusService or create a helper
            // This is a simplified implementation - in production, you'd use proper OAuth 1.0a signing
            // For the complete implementation, we need to add the token exchange logic to FanfouStatusService

            // TODO: Implement the OAuth access token exchange
            // This requires signing the request with OAuth 1.0a
            throw new NotImplementedException("Access token exchange needs to be implemented in FanfouStatusService");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during access token exchange: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}
