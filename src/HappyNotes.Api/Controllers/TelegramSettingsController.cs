using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyNotes.Api.Controllers;

public class TelegramSettingsController(
    IMapper mapper,
    CurrentUser currentUser,
    IRepositoryBase<TelegramSettings> telegramSyncSettingsRepository,
    IOptions<JwtConfig> jwtConfig
) : BaseController
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;

    [HttpGet]
    public async Task<ApiResult<List<TelegramSettingsDto>>> GetAll()
    {
        var userId = currentUser.Id;
        var settings = await telegramSyncSettingsRepository.GetListAsync(s => s.UserId.Equals(userId), o => o.CreatedAt);
        return new SuccessfulResult<List<TelegramSettingsDto>>(mapper.Map<List<TelegramSettingsDto>>(settings));
    }

    [HttpPost]
    public async Task<ApiResult<bool>> Add(TelegramSettingsDto settingsDto)
    {
        var userId = currentUser.Id;
        var existingSetting = await telegramSyncSettingsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.SyncType == settingsDto.SyncType && s.SyncValue == settingsDto.SyncValue);

        if (existingSetting != null)
        {
            throw new Exception(
                $"SyncType: {settingsDto.SyncType} has already been set to channel: {existingSetting.ChannelId}");
        }

        // todo: more verification before inserting the settings
        var now = DateTime.UtcNow.ToUnixTimeSeconds();
        var settings = new TelegramSettings()
        {
            UserId = userId,
            SyncType = settingsDto.SyncType,
            SyncValue = settingsDto.SyncValue,
            // one key is being used for two purposes, I know it is not good, but it is convenient.
            EncryptedToken =
                TextEncryptionHelper.Encrypt(settingsDto.EncryptedToken, _jwtConfig.SymmetricSecurityKey),
            TokenRemark = settingsDto.TokenRemark,
            ChannelId = settingsDto.ChannelId,
            CreatedAt = now,
        };
        var result = await telegramSyncSettingsRepository.InsertAsync(settings);
        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows inserted");
    }

    [HttpDelete]
    public async Task<ApiResult<bool>> Delete(TelegramSettingsDto settingsDto)
    {
        var userId = currentUser.Id;
        var existingSetting = await telegramSyncSettingsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.SyncType == settingsDto.SyncType && s.SyncValue == settingsDto.SyncValue);

        if (existingSetting == null)
        {
            throw new Exception(
                $"SyncType: {settingsDto.SyncType} has already been Deleted");
        }

        var result = await telegramSyncSettingsRepository.DeleteAsync(s => s.UserId == userId &&
                                                                           s.SyncType == existingSetting.SyncType &&
                                                                           s.SyncValue == existingSetting.SyncValue);
        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows inserted");
    }
}
