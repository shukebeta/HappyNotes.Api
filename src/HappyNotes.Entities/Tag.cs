using Api.Framework;
using SqlSugar;

namespace HappyNotes.Entities;
public class Tag : AdminEntityBase
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }
    [SugarColumn(IsPrimaryKey = true)]
    public string Name { get; set; } = string.Empty;
    public int PublicCount { get; set; }
    public int PrivateCount { get; set; }
    public int TotalCount { get; set; }
}
