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
        var settings = await userSettingsRepository.GetListAsync(s => s.UserId.Equals(currentUser.Id));
        if (settings.Count < DefaultValues.Settings.Count)
        {
            var userSettingDict = new Dictionary<string, string>(DefaultValues.SettingsDictionary);
            foreach (var setting in settings)
            {
                userSettingDict[setting.SettingName] = setting.SettingValue;
            }

            settings = userSettingDict.Select(kvp => new UserSettings()
            {
                UserId = currentUser.Id,
                SettingName = kvp.Key,
                SettingValue = kvp.Value
            }).ToList();
        }
        return new SuccessfulResult<List<UserSettingsDto>>(mapper.Map<List<UserSettingsDto>>(settings));
    }

    [HttpPost]
    public async Task<ApiResult<bool>> Setup(UserSettingsDto settingsDto)
    {
        if (!DefaultValues.SettingsDictionary.TryGetValue(settingsDto.SettingName, out _))
        {
            throw ExceptionHelper.New(settingsDto, EventId._00104_NoteIsNotDeleted, settingsDto.SettingName);
        }

        var now = DateTime.UtcNow.ToUnixTimestamp();
        var oldSetting = await userSettingsRepository.GetFirstOrDefaultAsync(
            s => s.UserId.Equals(currentUser.Id) &&
                 s.SettingName.Equals(settingsDto.SettingName));
        if (oldSetting != null)
        {
            if (settingsDto.SettingValue.Equals(oldSetting.SettingValue))
            {
                return new SuccessfulResult<bool>(true);
            }

            var result = await userSettingsRepository.UpdateAsync(it => new UserSettings()
            {
                SettingValue = settingsDto.SettingValue,
                UpdateAt = now,
            }, w => w.UserId.Equals(currentUser.Id) && w.SettingName.Equals(settingsDto.SettingName));
            return result > 0 ? new SuccessfulResult<bool>(true) : new FailedResult<bool>(false, "0 rows updated");
        }

        var settings = new UserSettings()
        {
            UserId = currentUser.Id,
            SettingName = settingsDto.SettingName,
            SettingValue = settingsDto.SettingValue,
            CreateAt = now,
        };
        await userSettingsRepository.InsertAsync(settings);
        return new SuccessfulResult<bool>(true);
    }
}
