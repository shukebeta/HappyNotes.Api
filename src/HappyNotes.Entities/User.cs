using Api.Framework;
using SqlSugar;

namespace HappyNotes.Entities;

public class User : EntityBase
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] public long Id { get; set; }

    [SugarColumn(IsPrimaryKey = true)] public string Username { get; set; } = string.Empty;

    [SugarColumn(IsPrimaryKey = true)] public string Email { get; set; } = string.Empty;

    public int EmailVerified { get; set; } = 0;

    public string Password { get; init; } = string.Empty;

    public string Salt { get; init; } = string.Empty;

    public string? Gravatar { get; set; } = string.Empty;
}
