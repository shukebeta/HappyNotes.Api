using HappyNotes.Entities;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Logging;

namespace HappyNotes.Services;

public class ManticoreSyncNoteService(
    ISearchService searchService,
    ILogger<ManticoreSyncNoteService> logger
) : ISyncNoteService
{
    public async Task SyncNewNote(Note note, string fullContent)
    {
        try
        {
            await searchService.SyncNoteToIndexAsync(note, fullContent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync new note to Manticore index: {NoteId}", note.Id);
        }
    }

    public async Task SyncEditNote(Note note, string fullContent)
    {
        try
        {
            await searchService.SyncNoteToIndexAsync(note, fullContent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync edited note to Manticore index: {NoteId}", note.Id);
        }
    }

    public async Task SyncDeleteNote(Note note)
    {
        try
        {
            await searchService.DeleteNoteFromIndexAsync(note.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete note from Manticore index: {NoteId}", note.Id);
        }
    }
}
