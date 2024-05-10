using System.ComponentModel;

namespace HappyNotes.Common;

public enum EventId
{
    [Description("Successful")]
    _00000_Successful = 0,
    [Description("Failed")]
    _00001_Failed = 1,
    [Description("Note with id {0} not found")]
    _00100_NoteNotFound = 100,
}
