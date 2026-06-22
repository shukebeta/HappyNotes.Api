using Api.Framework.Models;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using HappyNotes.Services.SyncQueue.Interfaces;
using HappyNotes.Services.SyncQueue.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyNotes.Services;

public class FanfouSyncNoteService(
    INoteRepository noteRepository,
    IFanfouUserAccountCacheService fanfouUserAccountCacheService,
    IOptions<JwtConfig> jwtConfig,
    ISyncQueueService syncQueueService,
    ILogger<FanfouSyncNoteService> logger
)
    : ISyncNoteService
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;

    public async Task SyncNewNote(Note note, string fullContent)
    {
        logger.LogInformation("Starting sync of new note {NoteId} for user {UserId}. ContentLength: {ContentLength}, IsPrivate: {IsPrivate}, IsMarkdown: {IsMarkdown}",
            note.Id, note.UserId, fullContent.Length, note.IsPrivate, note.IsMarkdown);

        try
        {
            var accounts = await _GetToSyncFanfouUserAccounts(note);
            logger.LogDebug("Found {AccountCount} Fanfou accounts to sync for note {NoteId}",
                accounts.Count, note.Id);

            if (accounts.Any())
            {
                // UNIFIED QUEUE MODE: Always use queue for consistent behavior
                foreach (var account in accounts)
                {
                    await EnqueueSyncTask(note, fullContent, account, "CREATE");
                    logger.LogDebug("Queued CREATE task for note {NoteId} to Fanfou account {AccountId}",
                        note.Id, account.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in SyncNewNote for note {NoteId}", note.Id);
        }
    }

    public async Task SyncEditNote(Note note, string fullContent, Note existingNote)
    {
        logger.LogInformation("Starting sync edit of note {NoteId} for user {UserId}. ContentLength: {ContentLength}, IsPrivate: {IsPrivate}, IsMarkdown: {IsMarkdown}",
            note.Id, note.UserId, fullContent.Length, note.IsPrivate, note.IsMarkdown);

        try
        {
            var hasContentChange = _HasContentChanged(existingNote, note, fullContent);
            var accounts = await fanfouUserAccountCacheService.GetAsync(note.UserId);
            if (!accounts.Any())
            {
                logger.LogDebug("No Fanfou accounts found for user {UserId}, skipping sync edit of note {NoteId}",
                    note.UserId, note.Id);
                return;
            }

            var accountData = await _GetAccountsData(note);
            var toBeSent = accountData.toBeSent;
            var tobeRemoved = accountData.toBeRemoved;
            var tobeUpdated = accountData.toBeUpdated;

            logger.LogDebug("Edit sync plan for note {NoteId}: {ToSendCount} to send, {ToUpdateCount} to update, {ToRemoveCount} to remove",
                note.Id, toBeSent.Count, tobeUpdated.Count, tobeRemoved.Count);

            foreach (var account in tobeRemoved)
            {
                var syncedStatuses = _GetSyncedStatuses(note);
                var status = syncedStatuses.FirstOrDefault(s => s.UserAccountId == account.Id);
                if (status != null)
                {
                    await EnqueueSyncTask(note, string.Empty, account, "DELETE", status.StatusId);
                    logger.LogDebug("Queued DELETE task for note {NoteId} from Fanfou account {AccountId}",
                        note.Id, account.Id);
                }
            }

            foreach (var account in tobeUpdated)
            {
                // Fanfou has no edit API, so we use DELETE + CREATE for content changes
                if (hasContentChange)
                {
                    var syncedStatuses = _GetSyncedStatuses(note);
                    var status = syncedStatuses.FirstOrDefault(s => s.UserAccountId == account.Id);
                    if (status != null)
                    {
                        // Queue DELETE for existing status
                        await EnqueueSyncTask(note, string.Empty, account, "DELETE", status.StatusId);
                        logger.LogDebug("Queued DELETE task for note {NoteId} from Fanfou account {AccountId} (content changed)",
                            note.Id, account.Id);

                        // Queue CREATE for new content
                        await EnqueueSyncTask(note, fullContent, account, "CREATE");
                        logger.LogDebug("Queued CREATE task for note {NoteId} to Fanfou account {AccountId} (content changed)",
                            note.Id, account.Id);
                    }
                }
            }

            foreach (var account in toBeSent)
            {
                await EnqueueSyncTask(note, fullContent, account, "CREATE");
                logger.LogDebug("Queued CREATE task for note {NoteId} to Fanfou account {AccountId}",
                    note.Id, account.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in SyncEditNote for note {NoteId}", note.Id);
        }
    }

    public async Task SyncDeleteNote(Note note)
    {
        logger.LogInformation("Starting sync delete of note {NoteId} for user {UserId}", note.Id, note.UserId);

        if (!string.IsNullOrWhiteSpace(note.FanfouStatusIds))
        {
            try
            {
                var accounts = await fanfouUserAccountCacheService.GetAsync(note.UserId);
                if (!accounts.Any())
                {
                    logger.LogDebug("No Fanfou accounts found for user {UserId}, skipping sync delete of note {NoteId}",
                        note.UserId, note.Id);
                    return;
                }

                var syncedStatuses = _GetSyncedStatuses(note);
                logger.LogDebug("Deleting note {NoteId} from {StatusCount} Fanfou statuses",
                    note.Id, syncedStatuses.Count);

                foreach (var status in syncedStatuses)
                {
                    var account = accounts.FirstOrDefault(s => s.Id.Equals(status.UserAccountId));
                    if (account != null)
                    {
                        await EnqueueSyncTask(note, string.Empty, account, "DELETE", status.StatusId);
                        logger.LogDebug("Queued DELETE task for note {NoteId} from Fanfou account {AccountId}",
                            note.Id, account.Id);
                    }
                    else
                    {
                        logger.LogWarning("Account {AccountId} not found for deleting status {StatusId} from note {NoteId}",
                            status.UserAccountId, status.StatusId, note.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error enqueueing Fanfou delete tasks for note {NoteId}", note.Id);
            }
        }
        else
        {
            logger.LogDebug("Note {NoteId} has no Fanfou sync data, nothing to delete", note.Id);
        }
    }

    public async Task SyncUndeleteNote(Note note)
    {
        await Task.CompletedTask;
    }

    public async Task PurgeDeletedNotes()
    {
        await Task.CompletedTask;
    }

    private async Task<(List<FanfouUserAccount> toBeUpdated, List<FanfouUserAccount> toBeRemoved,
        List<FanfouUserAccount> toBeSent)> _GetAccountsData(Note note)
    {
        var syncedStatuses = _GetSyncedStatuses(note);
        var toSyncAccounts = await _GetToSyncFanfouUserAccounts(note);

        var toBeUpdated = _GetAccountsToBeUpdated(syncedStatuses, toSyncAccounts);
        var toBeRemoved = _GetAccountsToBeRemoved(syncedStatuses, toSyncAccounts);
        var toBeSent = _GetAccountsToBeSent(syncedStatuses, toSyncAccounts);

        return (toBeUpdated, toBeRemoved, toBeSent);
    }

    private async Task<IList<FanfouUserAccount>> _GetToSyncFanfouUserAccounts(Note note)
    {
        var result = new List<FanfouUserAccount>();
        var all = await fanfouUserAccountCacheService.GetAsync(note.UserId);
        foreach (var account in all)
        {
            switch (account.SyncType)
            {
                case FanfouSyncType.All:
                    result.Add(account);
                    break;
                case FanfouSyncType.PublicOnly:
                    if (!note.IsPrivate)
                    {
                        result.Add(account);
                    }
                    break;
                case FanfouSyncType.TagFanfouOnly:
                    if (note.TagList.Contains("fanfou"))
                    {
                        result.Add(account);
                    }
                    break;
            }
        }

        return result;
    }

    private List<FanfouUserAccount> _GetAccountsToBeUpdated(List<FanfouSyncedStatus> statuses,
        IList<FanfouUserAccount> toSyncAccounts)
    {
        var idsToUpdate = statuses.Select(i => i.UserAccountId).Intersect(toSyncAccounts.Select(t => t.Id)).ToList();
        return toSyncAccounts.Where(r => idsToUpdate.Contains(r.Id)).ToList();
    }

    private List<FanfouUserAccount> _GetAccountsToBeRemoved(List<FanfouSyncedStatus> statuses,
        IList<FanfouUserAccount> toSyncAccounts)
    {
        if (!toSyncAccounts.Any()) return [];

        var idsToRemove = statuses.Select(s => s.UserAccountId).Except(toSyncAccounts.Select(t => t.Id)).ToList();
        return toSyncAccounts.Where(r => idsToRemove.Contains(r.Id)).ToList();
    }

    private List<FanfouUserAccount> _GetAccountsToBeSent(List<FanfouSyncedStatus> syncedStatuses,
        IList<FanfouUserAccount> toSyncAccounts)
    {
        if (!syncedStatuses.Any()) return toSyncAccounts.ToList();
        var toSendUserAccountId = toSyncAccounts.Select(t => t.Id).Except(syncedStatuses.Select(s => s.UserAccountId))
            .ToList();
        return toSyncAccounts.Where(t => toSendUserAccountId.Contains(t.Id)).ToList();
    }

    private async Task EnqueueSyncTask(Note note, string fullContent, FanfouUserAccount account, string action, string? statusId = null)
    {
        var payload = new FanfouSyncPayload
        {
            UserAccountId = account.Id,
            FullContent = fullContent,
            StatusId = statusId,
            IsPrivate = note.IsPrivate,
            IsMarkdown = note.IsMarkdown
        };

        var task = SyncTask.Create("fanfou", action, note.Id, note.UserId, payload);
        await syncQueueService.EnqueueAsync("fanfou", task);

        logger.LogInformation("Enqueued Fanfou sync task {TaskId} for note {NoteId}, action: {Action}, account: {AccountId}",
            task.Id, note.Id, action, account.Id);
    }

    private static List<FanfouSyncedStatus> _GetSyncedStatuses(Note note)
    {
        if (string.IsNullOrWhiteSpace(note.FanfouStatusIds)) return new List<FanfouSyncedStatus>();
        return note.FanfouStatusIds.Split(",").Select(s =>
        {
            var parts = s.Split(":");
            return new FanfouSyncedStatus
            {
                UserAccountId = long.Parse(parts[0]),
                StatusId = parts[1],
            };
        }).ToList();
    }

    private bool _HasContentChanged(Note existingNote, Note newNote, string newFullContent)
    {
        if (existingNote.Content != newFullContent)
        {
            return true;
        }

        if (existingNote.IsMarkdown != newNote.IsMarkdown)
        {
            return true;
        }

        return false;
    }
}

public class FanfouSyncedStatus
{
    public long UserAccountId { get; set; }
    public string StatusId { get; set; } = string.Empty;
}
