using HappyNotes.Dto;

namespace HappyNotes.Common;

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
            SettingName = "pagerIsFixed",
            SettingValue = "1",
        },

        new UserSettingsDto()
        {
            SettingName = "newNoteIsPublic",
            SettingValue = "1",
        },

        new UserSettingsDto()
        {
            SettingName = "markdownIsEnabled",
            SettingValue = "0",
        },
    ];

    public static Dictionary<string, string> SettingsDictionary =>
        Settings.ToDictionary(s => s.SettingName, v => v.SettingValue);
}
