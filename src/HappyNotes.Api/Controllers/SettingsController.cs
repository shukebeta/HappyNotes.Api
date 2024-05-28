using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using Microsoft.AspNetCore.Mvc;

namespace HappyNotes.Api.Controllers;

public class SettingsController(
    IMapper mapper,
    CurrentUser currentUser,
    IRepositoryBase<UserSettings> userSettingsRepository) : BaseController
{
    [HttpGet]
    public async Task<ApiResult<List<UserSettingsDto>>> Get()
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

    [HttpPost]
    public async Task<ApiResult<bool>> Setup(UserSettingsDto settingsDto)
    {
        var userId = currentUser.Id;
        if (!DefaultValues.SettingsDictionary.ContainsKey(settingsDto.SettingName))
        {
            throw ExceptionHelper.New(settingsDto, EventId._00104_NoteIsNotDeleted, settingsDto.SettingName);
        }

        var now = DateTime.UtcNow.ToUnixTimestamp();
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
