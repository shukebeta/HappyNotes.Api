using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyNotes.Api.Controllers;

public class MastodonSettingsController(
    IMapper mapper,
    CurrentUser currentUser,
    IRepositoryBase<MastodonUserAccount> mastodonUserAccountsRepository,
    IMastodonUserAccountCacheService mastodonUserAccountsCacheService,
    // ITelegramService telegramService,
    IOptions<JwtConfig> jwtConfig
) : BaseController
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;

    [HttpGet]
    public async Task<ApiResult<List<MastodonUserAccount>>> GetAll()
    {
        var userId = currentUser.Id;
        var settings = await mastodonUserAccountsRepository.GetListAsync(s => s.UserId.Equals(userId), o => o.CreatedAt);
        return new SuccessfulResult<List<MastodonUserAccount>>(mapper.Map<List<MastodonUserAccount>>(settings));
    }

    [HttpPost]
    public async Task<ApiResult<bool>> Add(MastodonUserAccount mastodonUserAccount)
    {
        var userId = currentUser.Id;
        var existingSetting = await mastodonUserAccountsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.ApplicationId == mastodonUserAccount.ApplicationId);

        if (existingSetting != null)
        {
            throw new Exception(
                $"Mastodon instance: {mastodonUserAccount.InstanceUrl} has already been set");
        }

        // todo: more verification before inserting the settings
        var now = DateTime.UtcNow.ToUnixTimeSeconds();
        var settings = new MastodonUserAccount()
        {
            UserId = userId,
            ApplicationId = mastodonUserAccount.ApplicationId,
            InstanceUrl = mastodonUserAccount.InstanceUrl,
            TokenType = mastodonUserAccount.TokenType,
            // _jwtConfig.SymmetricSecurityKey is being used for two purposes, I know it is not good, but it is convenient.
            AccessToken =
                TextEncryptionHelper.Encrypt(mastodonUserAccount.AccessToken,  _jwtConfig.SymmetricSecurityKey),
            RefreshToken =
                TextEncryptionHelper.Encrypt(mastodonUserAccount.RefreshToken,  _jwtConfig.SymmetricSecurityKey),
            ExpiresAt = mastodonUserAccount.ExpiresAt,
            Username = mastodonUserAccount.Username,
            DisplayName = mastodonUserAccount.DisplayName,
            AvatarUrl = mastodonUserAccount.AvatarUrl,
            Scope = mastodonUserAccount.Scope,
            MastodonUserId = mastodonUserAccount.MastodonUserId,
            Status = MastodonUserAccountStatus.Created,
            StatusText = "Created successfully",
            CreatedAt = now,
        };

        var result = await mastodonUserAccountsRepository.InsertAsync(settings);
        if (result) mastodonUserAccountsCacheService.ClearCache(userId);
        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows inserted");
    }

    [HttpPost]
    public async Task<ApiResult<bool>> Disable(MastodonUserAccount mastodonUserAccount)
    {
        var userId = currentUser.Id;
        var existingSetting = await mastodonUserAccountsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.ApplicationId == mastodonUserAccount.ApplicationId);
        if (existingSetting == null)
        {
            throw new Exception(
                $"Setting with {mastodonUserAccount.Id} doesn't exist.");
        }

        if (existingSetting.Status.Has(MastodonUserAccountStatus.Inactive))
        {
            throw new Exception(
                $"Setting with {mastodonUserAccount.Id} is already Disabled");
        }

        existingSetting.Status = existingSetting.Status.Add(MastodonUserAccountStatus.Inactive);

        var result = await mastodonUserAccountsRepository.UpdateAsync(existingSetting);
        if (result) mastodonUserAccountsCacheService.ClearCache(userId);
        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows Updated");
    }

    [HttpPost]
    public async Task<ApiResult<bool>> Activate(MastodonUserAccount mastodonUserAccount)
    {
        var userId = currentUser.Id;
        var existingSetting = await mastodonUserAccountsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.ApplicationId == mastodonUserAccount.ApplicationId);

        if (existingSetting == null)
        {
            throw new Exception(
                $"Setting with {mastodonUserAccount.ApplicationId} doesn't exist.");
        }

        if (!existingSetting.Status.Has(TelegramSettingStatus.Inactive))
        {
            throw new Exception(
                $"Setting with {mastodonUserAccount.Id} is already active");
        }

        existingSetting.Status = existingSetting.Status.Remove(MastodonUserAccountStatus.Inactive);

        var result = await mastodonUserAccountsRepository.UpdateAsync(existingSetting);
        if (result) mastodonUserAccountsCacheService.ClearCache(userId);
        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows Updated");
    }

    [HttpDelete]
    public async Task<ApiResult<bool>> Delete(MastodonUserAccount mastodonUserAccount)
    {
        var userId = currentUser.Id;
        var existingSetting = await mastodonUserAccountsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.ApplicationId == mastodonUserAccount.ApplicationId);

        if (existingSetting == null)
        {
            throw new Exception(
                $"Mastodon instance: {mastodonUserAccount.InstanceUrl} has already been Deleted");
        }

        var result = await mastodonUserAccountsRepository.DeleteAsync(s => s.UserId == userId &&
                                                                           s.ApplicationId == existingSetting.ApplicationId);
        if (result) mastodonUserAccountsCacheService.ClearCache(userId);
        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows deleted");
    }

    [HttpPost]
    public async Task<ApiResult<bool>> Test(MastodonUserAccount mastodonUserAccount)
    {
        var userId = currentUser.Id;
        var existingSetting = await mastodonUserAccountsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.ApplicationId == mastodonUserAccount.ApplicationId);

        if (existingSetting == null)
        {
            throw new Exception( $"Could find this setting.");
        }
        // if (string.IsNullOrWhiteSpace(existingSetting.EncryptedToken) || string.IsNullOrWhiteSpace(existingSetting.ChannelId))
        // {
        //     throw new Exception( $"token or channelId is empty, cannot test.");
        // }
        // var token = TextEncryptionHelper.Decrypt(existingSetting.EncryptedToken, _jwtConfig.SymmetricSecurityKey);
        // var message = await telegramService.SendMessageAsync(token, existingSetting.ChannelId, "Hello *world!*", true);
        // if (message.MessageId > 0)
        // {
        //     existingSetting.Status = existingSetting.Status.Add(TelegramSettingStatus.Tested);
        //     await mastodonUserAccountsRepository.UpdateAsync(existingSetting);
        // }
        // return message.MessageId > 0 ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "test failed.");
        return new FailedResult<bool>(false, "test failed");
    }
}
