using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyNotes.Api.Controllers;

public class SettingsController(
    IMapper mapper,
    CurrentUser currentUser,
    IRepositoryBase<UserSettings> userSettingsRepository,
    IRepositoryBase<TelegramSyncSettings> telegramSyncSettingsRepository,
    IOptions<JwtConfig> jwtConfig
) : BaseController
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;

    [HttpGet]
    public async Task<ApiResult<List<UserSettingsDto>>> GetAll()
    {
        var userId = currentUser.Id;
        var settings = await userSettingsRepository.GetListAsync(s => s.UserId.Equals(userId));
        if (settings.Count < DefaultValues.Settings.Count)
        {
            var userSettingDict = new Dictionary<string, string>(DefaultValues.SettingsDictionary);
            foreach (var setting in settings)
            {
                userSettingDict[setting.SettingName] = setting.SettingValue;
            }

            settings = userSettingDict.Select(kvp => new UserSettings()
            {
                UserId = userId,
                SettingName = kvp.Key,
                SettingValue = kvp.Value
            }).ToList();
        }

        return new SuccessfulResult<List<UserSettingsDto>>(mapper.Map<List<UserSettingsDto>>(settings));
    }

    [HttpGet]
    public async Task<ApiResult<List<TelegramSyncSettingsDto>>> GetSyncSettings()
    {
        var userId = currentUser.Id;
        var settings = await telegramSyncSettingsRepository.GetListAsync(s => s.UserId.Equals(userId), o => o.CreateAt);
        return new SuccessfulResult<List<TelegramSyncSettingsDto>>(mapper.Map<List<TelegramSyncSettingsDto>>(settings));
    }

    [HttpPost]
    public async Task<ApiResult<bool>> AddSync(TelegramSyncSettingsDto settingsDto)
    {
        var userId = currentUser.Id;
        var existingSetting = await telegramSyncSettingsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.SyncType == settingsDto.SyncType && s.SyncValue == settingsDto.SyncValue);

        if (existingSetting != null)
        {
            throw new Exception(
                $"SyncType: {settingsDto.SyncType} has already been set to channel: {existingSetting.TelegramChannelId}");
        }

        // todo: more verification before inserting the settings
        var now = DateTime.UtcNow.ToUnixTimeSeconds();
        var settings = new TelegramSyncSettings()
        {
            UserId = userId,
            SyncType = settingsDto.SyncType,
            SyncValue = settingsDto.SyncValue,
            // one key is being used for two purposes, I know it is not good, but it is convenient.
            EncryptedTelegramToken =
                TextEncryptionHelper.Encrypt(settingsDto.EncryptedTelegramToken, _jwtConfig.SymmetricSecurityKey),
            TokenRemark = settingsDto.TokenRemark,
            TelegramChannelId = settingsDto.TelegramChannelId,
            CreateAt = now,
        };
        var result = await telegramSyncSettingsRepository.InsertAsync(settings);
        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows inserted");
    }

    [HttpDelete]
    public async Task<ApiResult<bool>> DeleteSync(TelegramSyncSettingsDto settingsDto)
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

    /// <summary>
    /// Upserts the user settings.
    /// </summary>
    /// <param name="settingsDto">The settings DTO.</param>
    /// <returns>Returns a result indicating success or failure.</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Upsert(UserSettingsDto settingsDto)
    {
        var userId = currentUser.Id;
        if (!DefaultValues.SettingsDictionary.ContainsKey(settingsDto.SettingName))
        {
            throw ExceptionHelper.New(settingsDto, EventId._00106_UnknownSettingName, settingsDto.SettingName);
        }

        var now = DateTime.UtcNow.ToUnixTimeSeconds();
        var existingSetting = await userSettingsRepository.GetFirstOrDefaultAsync(
            s => s.UserId == userId && s.SettingName == settingsDto.SettingName);

        bool result;
        if (existingSetting != null)
        {
            if (settingsDto.SettingValue == existingSetting.SettingValue)
            {
                return new SuccessfulResult<bool>(true);
            }

            result = await _UpdateSetting(userId, settingsDto.SettingName, settingsDto.SettingValue, now);
        }
        else
        {
            result = await _InsertSetting(userId, settingsDto.SettingName, settingsDto.SettingValue, now);
        }

        return result ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows updated");
    }

    private async Task<bool> _UpdateSetting(long userId, string settingName, string settingValue, long timestamp)
    {
        var result = await userSettingsRepository.UpdateAsync(
            it => new UserSettings()
            {
                SettingValue = settingValue,
                UpdateAt = timestamp,
            },
            w => w.UserId == userId && w.SettingName == settingName);
        return result > 0;
    }

    private async Task<bool> _InsertSetting(long userId, string settingName, string settingValue, long timestamp)
    {
        var settings = new UserSettings()
        {
            UserId = userId,
            SettingName = settingName,
            SettingValue = settingValue,
            CreateAt = timestamp,
        };
        return await userSettingsRepository.InsertAsync(settings);
    }
}
