using Api.Framework;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Services;
using HappyNotes.Services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

public class MastodonUserAccountController(
    IMapper mapper,
    ICurrentUser currentUser,
    IRepositoryBase<MastodonUserAccount> mastodonUserAccountsRepository,
    IMastodonUserAccountCacheService mastodonUserAccountsCacheService
) : BaseController
{
    [HttpGet]
    public async Task<ApiResult<List<MastodonUserAccount>>> GetAll()
    {
        var userId = currentUser.Id;
        var settings = await mastodonUserAccountsRepository.GetListAsync(s => s.UserId.Equals(userId), o => o.CreatedAt);
        return new SuccessfulResult<List<MastodonUserAccount>>(mapper.Map<List<MastodonUserAccount>>(settings));
    }

    [HttpPost]
    public async Task<ApiResult> Disable(MastodonUserAccount mastodonUserAccount)
    {
        var userId = currentUser.Id;
        var existingSetting = await mastodonUserAccountsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.InstanceUrl == mastodonUserAccount.InstanceUrl);
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
        return result ? Success() : new FailedResult<bool>(false, "0 rows Updated");
    }

    [HttpPost]
    public async Task<ApiResult<bool>> NextSyncType(MastodonUserAccount mastodonUserAccount)
    {
        var userId = currentUser.Id;
        var existingSetting = await mastodonUserAccountsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.InstanceUrl == mastodonUserAccount.InstanceUrl);

        if (existingSetting == null)
        {
            throw new Exception(
                $"Setting with {mastodonUserAccount.InstanceUrl} doesn't exist.");
        }

        existingSetting.SyncType = existingSetting.SyncType.Next();
        var result = await mastodonUserAccountsRepository.UpdateAsync(existingSetting);
        if (result) mastodonUserAccountsCacheService.ClearCache(userId);
        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows Updated");
    }

    [HttpPost]
    public async Task<ApiResult<bool>> Activate(MastodonUserAccount mastodonUserAccount)
    {
        var userId = currentUser.Id;
        var existingSetting = await mastodonUserAccountsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.InstanceUrl == mastodonUserAccount.InstanceUrl);

        if (existingSetting == null)
        {
            throw new Exception(
                $"Setting with {mastodonUserAccount.InstanceUrl} doesn't exist.");
        }

        if (!existingSetting.Status.Has(TelegramSettingStatus.Inactive))
        {
            throw new Exception(
                $"Setting with {mastodonUserAccount.InstanceUrl} is already active");
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
            s => s.UserId == userId && s.InstanceUrl == mastodonUserAccount.InstanceUrl);

        if (existingSetting == null)
        {
            throw new Exception(
                $"Mastodon instance: {mastodonUserAccount.InstanceUrl} has already been Deleted");
        }

        var result = await mastodonUserAccountsRepository.DeleteAsync(s => s.UserId == userId &&
                                                                           s.InstanceUrl == existingSetting.InstanceUrl);
        if (result) mastodonUserAccountsCacheService.ClearCache(userId);
        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows deleted");
    }

}
