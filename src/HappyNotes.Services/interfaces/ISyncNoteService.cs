using HappyNotes.Entities;

namespace HappyNotes.Services.interfaces;

public interface ISyncNoteService
{
    Task SyncNewNote(Note note, string fullContent);
    Task SyncEditNote(Note note, string fullContent);
    Task SyncDeleteNote(Note note);
}
