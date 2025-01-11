namespace HappyNotes.Dto;

public class UserSettingsDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
}
