using System.ComponentModel;

namespace HappyNotes.Common.Enums;

public enum NoteType
{
    [Description("All notes: include private and public")]
    All = 0,
    [Description("Private Notes only")]
    Private = 1,
    [Description("Public Notes only")]
    Public = 2,
}
