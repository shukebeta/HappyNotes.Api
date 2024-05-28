using Api.Framework;
using SqlSugar;

namespace HappyNotes.Entities;

public class UserSettings
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    public long UserId { get; set; }
    public string SettingName { get; set; }
    public string SettingValue { get; set; }
    public long CreateAt { get; set; }
    public long DeleteAt { get; set; }
    public long UpdateAt { get; set; }
}
