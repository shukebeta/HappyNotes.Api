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
    public long CreatedAt { get; set; }
    public long DeletedAt { get; set; }
    public long UpdatedAt { get; set; }
}
