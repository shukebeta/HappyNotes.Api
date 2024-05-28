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
    [Description("Note with id {0} is private")]
    _00101_NoteIsPrivate = 101,
    [Description("Note with id {0} is not yours")]
    _00102_NoteIsNotYours = 102,
    [Description("Invalid operation: note with id {0} is not deleted")]
    _00103_NoteIsNotDeleted = 103,
    [Description("Invalid setting name: {0}")]
    _00104_NoteIsNotDeleted = 104,
}
