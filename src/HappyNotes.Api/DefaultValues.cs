using HappyNotes.Dto;

namespace HappyNotes.Api;

public static class DefaultValues
{
    public static readonly List<UserSettingsDto> Settings =
    [
        new UserSettingsDto()
        {
            SettingName = "pageSize",
            SettingValue = "20",
        },

        new UserSettingsDto()
        {
            SettingName = "markdownIsEnabled",
            SettingValue = "0",
        },

        new UserSettingsDto()
        {
            SettingName = "timezone",
            SettingValue = "Pacific/Auckland",
        },
    ];

    public static Dictionary<string, string> SettingsDictionary =>
        Settings.ToDictionary(s => s.SettingName, v => v.SettingValue);
}
