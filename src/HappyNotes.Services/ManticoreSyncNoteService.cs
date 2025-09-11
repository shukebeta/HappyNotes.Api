using HappyNotes.Entities;
using HappyNotes.Services.interfaces;
using HappyNotes.Services.SyncQueue.Interfaces;
using HappyNotes.Services.SyncQueue.Models;
using Microsoft.Extensions.Logging;

namespace HappyNotes.Services;

public class ManticoreSyncNoteService(
    ISyncQueueService syncQueueService,
    ILogger<ManticoreSyncNoteService> logger
) : ISyncNoteService
{
    public async Task SyncNewNote(Note note, string fullContent)
    {
        try
        {
            var payload = new ManticoreSearchSyncPayload
            {
                Action = "CREATE",
                FullContent = fullContent
            };

            var task = SyncTask.Create("manticoresearch", "CREATE", note.Id, note.UserId, payload);
            await syncQueueService.EnqueueAsync("manticoresearch", task);

            logger.LogDebug("Successfully queued ManticoreSearch CREATE for note {NoteId}", note.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to queue new note sync to ManticoreSearch: {NoteId}", note.Id);
        }
    }

    public async Task SyncEditNote(Note note, string fullContent)
    {
        try
        {
            var payload = new ManticoreSearchSyncPayload
            {
                Action = "UPDATE",
                FullContent = fullContent
            };

            var task = SyncTask.Create("manticoresearch", "UPDATE", note.Id, note.UserId, payload);
            await syncQueueService.EnqueueAsync("manticoresearch", task);

            logger.LogDebug("Successfully queued ManticoreSearch UPDATE for note {NoteId}", note.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to queue edited note sync to ManticoreSearch: {NoteId}", note.Id);
        }
    }

    public async Task SyncDeleteNote(Note note)
    {
        try
        {
            var payload = new ManticoreSearchSyncPayload
            {
                Action = "DELETE",
                FullContent = string.Empty // Not needed for delete operations
            };

            var task = SyncTask.Create("manticoresearch", "DELETE", note.Id, note.UserId, payload);
            await syncQueueService.EnqueueAsync("manticoresearch", task);

            logger.LogDebug("Successfully queued ManticoreSearch DELETE for note {NoteId}", note.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to queue delete note sync to ManticoreSearch: {NoteId}", note.Id);
        }
    }

    public async Task SyncUndeleteNote(Note note)
    {
        try
        {
            var payload = new ManticoreSearchSyncPayload
            {
                Action = "UNDELETE",
                FullContent = string.Empty // Not needed for undelete operations
            };

            var task = SyncTask.Create("manticoresearch", "UNDELETE", note.Id, note.UserId, payload);
            await syncQueueService.EnqueueAsync("manticoresearch", task);

            logger.LogDebug("Successfully queued ManticoreSearch UNDELETE for note {NoteId}", note.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to queue undelete note sync to ManticoreSearch: {NoteId}", note.Id);
        }
    }

    public async Task PurgeDeletedNotes()
    {
        // Note: PurgeDeletedNotes is a bulk operation that doesn't operate on individual notes
        // For now, keep direct call to SearchService as it's not tied to specific note operations
        // This could be migrated to queue later if needed for consistency
        logger.LogInformation("PurgeDeletedNotes not yet migrated to queue - this is a bulk operation");

        // TODO: Consider implementing bulk queue operations or keeping direct calls for maintenance operations
        await Task.CompletedTask;
    }
}
