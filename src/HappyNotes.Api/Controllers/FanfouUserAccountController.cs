using Api.Framework;
using Api.Framework.Helper;
using Api.Framework.Result;
using HappyNotes.Common.Enums;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyNotes.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class FanfouAccountsController(
    IRepositoryBase<FanfouUserAccount> fanfouUserAccountRepository,
    IFanfouUserAccountCacheService fanfouUserAccountCacheService,
    ICurrentUser currentUser,
    ILogger<FanfouAccountsController> logger) : BaseController
{
    [HttpGet]
    public async Task<ApiResult<List<FanfouUserAccountDto>>> GetAccounts()
    {
        try
        {
            var accounts = await fanfouUserAccountRepository.GetListAsync(a => a.UserId == currentUser.Id);

            var dtos = accounts.Select(a => new FanfouUserAccountDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Username = a.Username,
                SyncType = a.SyncType,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            }).ToList();

            logger.LogDebug("Retrieved {AccountCount} Fanfou accounts for user {UserId}",
                dtos.Count, currentUser.Id);

            return new SuccessfulResult<List<FanfouUserAccountDto>>(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting Fanfou accounts for user {UserId}", currentUser.Id);
            return new FailedResult<List<FanfouUserAccountDto>>(null, $"Failed to retrieve accounts: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ApiResult> AddAccount([FromBody] PostFanfouAccountRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ConsumerKey) || string.IsNullOrEmpty(request.ConsumerSecret))
            {
                return Fail("Consumer key and secret are required");
            }

            // Create new account (will be activated after OAuth flow completes)
            var account = new FanfouUserAccount
            {
                UserId = currentUser.Id,
                Username = "Pending", // Will be updated after OAuth
                ConsumerKey = TextEncryptionHelper.Encrypt(request.ConsumerKey, "TEMP_KEY"), // Will be re-encrypted
                ConsumerSecret = TextEncryptionHelper.Encrypt(request.ConsumerSecret, "TEMP_KEY"), // Will be re-encrypted
                AccessToken = string.Empty,
                AccessTokenSecret = string.Empty,
                SyncType = request.SyncType,
                Status = FanfouUserAccountStatus.Disabled,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };

            await fanfouUserAccountRepository.InsertAsync(account);

            // Clear cache
            fanfouUserAccountCacheService.ClearCache(currentUser.Id);

            logger.LogInformation("Created new Fanfou account for user {UserId}, pending OAuth", currentUser.Id);

            return Success("Account created. Please complete OAuth authorization.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding Fanfou account for user {UserId}", currentUser.Id);
            return Fail($"Failed to add account: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ApiResult> UpdateAccount(long id, [FromBody] PutFanfouAccountRequest request)
    {
        try
        {
            var account = await fanfouUserAccountRepository.GetFirstOrDefaultAsync(a => a.Id == id && a.UserId == currentUser.Id);
            if (account == null)
            {
                return Fail("Account not found");
            }

            account.SyncType = request.SyncType;
            await fanfouUserAccountRepository.UpdateAsync(account);

            // Clear cache
            fanfouUserAccountCacheService.ClearCache(currentUser.Id);

            logger.LogInformation("Updated Fanfou account {AccountId} sync type to {SyncType} for user {UserId}",
                id, request.SyncType, currentUser.Id);

            return Success("Account updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating Fanfou account {AccountId} for user {UserId}", id, currentUser.Id);
            return Fail($"Failed to update account: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> DeleteAccount(long id)
    {
        try
        {
            var account = await fanfouUserAccountRepository.GetFirstOrDefaultAsync(a => a.Id == id && a.UserId == currentUser.Id);
            if (account == null)
            {
                return Fail("Account not found");
            }

            await fanfouUserAccountRepository.DeleteAsync(account);

            // Clear cache
            fanfouUserAccountCacheService.ClearCache(currentUser.Id);

            logger.LogInformation("Deleted Fanfou account {AccountId} for user {UserId}", id, currentUser.Id);

            return Success("Account deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting Fanfou account {AccountId} for user {UserId}", id, currentUser.Id);
            return Fail($"Failed to delete account: {ex.Message}");
        }
    }
}
