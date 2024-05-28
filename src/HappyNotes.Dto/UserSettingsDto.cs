namespace HappyNotes.Dto;

public class UserSettingsDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string SettingName { get; set; }
    public string SettingValue { get; set; }
}
