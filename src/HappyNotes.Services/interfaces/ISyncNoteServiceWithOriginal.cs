using HappyNotes.Entities;

namespace HappyNotes.Services.interfaces;

public interface ISyncNoteServiceWithOriginal : ISyncNoteService
{
    Task SyncEditNote(Note note, string fullContent, Note originalNote);
}
