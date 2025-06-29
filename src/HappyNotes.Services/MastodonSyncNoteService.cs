using Api.Framework.Models;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;

namespace HappyNotes.Services;

public class MastodonSyncNoteService(
    IMastodonTootService mastodonTootService,
    INoteRepository noteRepository,
    IMastodonUserAccountCacheService mastodonUserAccountCacheService,
    IOptions<JwtConfig> jwtConfig,
    ILogger<MastodonSyncNoteService> logger
)
    : ISyncNoteService
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;
    private string TokenKey => _jwtConfig.SymmetricSecurityKey;

    public async Task SyncNewNote(Note note, string fullContent)
    {
        logger.LogInformation("Starting sync of new note {NoteId} for user {UserId}. ContentLength: {ContentLength}, IsPrivate: {IsPrivate}, IsMarkdown: {IsMarkdown}",
            note.Id, note.UserId, fullContent.Length, note.IsPrivate, note.IsMarkdown);

        var syncedInstances = new List<MastodonSyncedInstance>();
        try
        {
            var accounts = await _GetToSyncMastodonUserAccounts(note);
            logger.LogDebug("Found {AccountCount} Mastodon accounts to sync for note {NoteId}",
                accounts.Count, note.Id);

            if (accounts.Any())
            {
                foreach (var account in accounts)
                {
                    logger.LogDebug("Syncing note {NoteId} to account {AccountId} ({InstanceUrl})",
                        note.Id, account.Id, account.InstanceUrl);

                    try
                    {
                        var tootId = await _SentNoteToMastodon(note, fullContent, account);
                        syncedInstances.Add(new MastodonSyncedInstance
                        {
                            UserAccountId = account.Id,
                            TootId = tootId,
                        });

                        logger.LogDebug("Successfully synced note {NoteId} to {InstanceUrl}, tootId: {TootId}",
                            note.Id, account.InstanceUrl, tootId);
                    }
                    catch (Exception accountEx)
                    {
                        logger.LogError(accountEx, "Failed to sync note {NoteId} to account {AccountId} ({InstanceUrl}): {ErrorMessage}",
                            note.Id, account.Id, account.InstanceUrl, accountEx.Message);
                        // Continue with other accounts even if one fails
                    }
                }
            }

            note.UpdateMastodonInstanceIds(syncedInstances);
            await noteRepository.UpdateAsync(note);

            logger.LogInformation("Completed sync of note {NoteId}. Successfully synced to {SyncedCount}/{TotalCount} accounts",
                note.Id, syncedInstances.Count, accounts.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync new note {NoteId} for user {UserId}: {ErrorMessage}",
                note.Id, note.UserId, ex.Message);
        }
    }

    public async Task SyncEditNote(Note note, string fullContent)
    {
        logger.LogInformation("Starting sync edit of note {NoteId} for user {UserId}. ContentLength: {ContentLength}, IsPrivate: {IsPrivate}, IsMarkdown: {IsMarkdown}",
            note.Id, note.UserId, fullContent.Length, note.IsPrivate, note.IsMarkdown);

        var accounts = await mastodonUserAccountCacheService.GetAsync(note.UserId);
        if (!accounts.Any())
        {
            logger.LogDebug("No Mastodon accounts found for user {UserId}, skipping sync edit of note {NoteId}",
                note.UserId, note.Id);
            return;
        }

        logger.LogDebug("Found {AccountCount} Mastodon accounts for user {UserId}", accounts.Count, note.UserId);
        var instances = new List<MastodonSyncedInstance>();

        try
        {
            var instanceData = await _GetInstancesData(note);
            var toBeSent = instanceData.toBeSent;
            var tobeRemoved = instanceData.toBeRemoved;
            var tobeUpdated = instanceData.toBeUpdated;

            logger.LogDebug("Edit sync plan for note {NoteId}: {ToSendCount} to send, {ToUpdateCount} to update, {ToRemoveCount} to remove",
                note.Id, toBeSent.Count, tobeUpdated.Count, tobeRemoved.Count);

            // there are existing synced channels
            if (!string.IsNullOrWhiteSpace(note.MastodonTootIds))
            {
                // Remove toots that should no longer be synced
                foreach (var instance in tobeRemoved)
                {
                    var account = accounts.FirstOrDefault(s => s.Id.Equals(instance.UserAccountId));
                    if (account != null)
                    {
                        logger.LogDebug("Removing toot {TootId} from {InstanceUrl} for note {NoteId}",
                            instance.TootId, account.InstanceUrl, note.Id);
                        await _DeleteToot(instance, account);
                    }
                }

                // Update existing toots
                foreach (var instance in tobeUpdated)
                {
                    var userAccountId = instance.UserAccountId;
                    var account = accounts.FirstOrDefault(s => s.Id.Equals(userAccountId));
                    if (account == null) continue;

                    try
                    {
                        logger.LogDebug("Updating toot {TootId} on {InstanceUrl} for note {NoteId}",
                            instance.TootId, account.InstanceUrl, note.Id);

                        await mastodonTootService.EditTootAsync(account.InstanceUrl,
                            account.DecryptedAccessToken(TokenKey), instance.TootId,
                            fullContent,
                            note.IsPrivate, note.IsMarkdown);
                        instances.Add(new MastodonSyncedInstance
                        {
                            UserAccountId = userAccountId,
                            TootId = instance.TootId,
                        });

                        logger.LogDebug("Successfully updated toot {TootId} on {InstanceUrl} for note {NoteId}",
                            instance.TootId, account.InstanceUrl, note.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to update toot {TootId} on {InstanceUrl} for note {NoteId}: {ErrorMessage}",
                            instance.TootId, account.InstanceUrl, note.Id, ex.Message);
                        throw;
                    }
                }
            }

            // Send to new accounts
            foreach (var account in toBeSent)
            {
                try
                {
                    logger.LogDebug("Sending note {NoteId} to new account {AccountId} ({InstanceUrl})",
                        note.Id, account.Id, account.InstanceUrl);

                    var tootId = await _SentNoteToMastodon(note, fullContent, account);
                    instances.Add(new MastodonSyncedInstance
                    {
                        UserAccountId = account.Id,
                        TootId = tootId,
                    });

                    logger.LogDebug("Successfully sent note {NoteId} to {InstanceUrl}, tootId: {TootId}",
                        note.Id, account.InstanceUrl, tootId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send note {NoteId} to account {AccountId} ({InstanceUrl}): {ErrorMessage}",
                        note.Id, account.Id, account.InstanceUrl, ex.Message);
                    // Continue with other accounts even if one fails
                }
            }

            note.UpdateMastodonInstanceIds(instances);
            await noteRepository.UpdateAsync(_ => new Note { MastodonTootIds = note.MastodonTootIds, },
                n => n.Id == note.Id);

            logger.LogInformation("Completed sync edit of note {NoteId}. Final synced instances: {SyncedCount}",
                note.Id, instances.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync edit note {NoteId} for user {UserId}: {ErrorMessage}",
                note.Id, note.UserId, ex.Message);
        }
    }

    public async Task SyncDeleteNote(Note note)
    {
        logger.LogInformation("Starting sync delete of note {NoteId} for user {UserId}", note.Id, note.UserId);

        if (!string.IsNullOrWhiteSpace(note.MastodonTootIds))
        {
            try
            {
                var accounts = await mastodonUserAccountCacheService.GetAsync(note.UserId);

                if (!accounts.Any())
                {
                    logger.LogDebug("No Mastodon accounts found for user {UserId}, skipping sync delete of note {NoteId}",
                        note.UserId, note.Id);
                    return;
                }

                var syncedInstances = _GetSyncedInstances(note);
                logger.LogDebug("Deleting note {NoteId} from {InstanceCount} Mastodon instances",
                    note.Id, syncedInstances.Count);

                foreach (var instance in syncedInstances)
                {
                    var userAccountId = instance.UserAccountId;
                    var account = accounts.FirstOrDefault(s => s.Id.Equals(userAccountId));
                    if (account == null)
                    {
                        logger.LogWarning("Account {AccountId} not found for deleting toot {TootId} from note {NoteId}",
                            userAccountId, instance.TootId, note.Id);
                        continue;
                    }

                    logger.LogDebug("Deleting toot {TootId} from {InstanceUrl} for note {NoteId}",
                        instance.TootId, account.InstanceUrl, note.Id);
                    await _DeleteToot(instance, account);
                }

                note.MastodonTootIds = null;
                await noteRepository.UpdateAsync(note);

                logger.LogInformation("Completed sync delete of note {NoteId} from all Mastodon instances", note.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync delete note {NoteId} for user {UserId}: {ErrorMessage}",
                    note.Id, note.UserId, ex.Message);
            }
        }
        else
        {
            logger.LogDebug("Note {NoteId} has no Mastodon sync data, nothing to delete", note.Id);
        }
    }

    public async Task SyncUndeleteNote(Note note)
    {
        // As per user instruction, no action is taken for undeleting notes on Mastodon
        await Task.CompletedTask;
    }

    public async Task PurgeDeletedNotes()
    {
        // As per user instruction, no action is taken for purging deleted notes on Mastodon
        await Task.CompletedTask;
    }

    private async Task<string> _SentNoteToMastodon(Note note, string fullContent, MastodonUserAccount account)
    {
        var toot = await mastodonTootService.SendTootAsync(account.InstanceUrl,
                account.DecryptedAccessToken(TokenKey), fullContent, note.IsPrivate, note.IsMarkdown);

        return toot.Id;
    }

    private async
        Task<(List<MastodonSyncedInstance> toBeUpdated, List<MastodonSyncedInstance> toBeRemoved,
            List<MastodonUserAccount> toBeSent)> _GetInstancesData(Note note)
    {
        var syncedChannels = _GetSyncedInstances(note);
        var toSyncInstances = await _GetToSyncMastodonUserAccounts(note);

        var toBeUpdated = _getInstancesToBeUpdated(syncedChannels, toSyncInstances);
        var toBeRemoved = _GetInstancesToBeRemoved(syncedChannels, toSyncInstances);
        var toBeSent = _GetAccountsToBeSent(syncedChannels, toSyncInstances);

        return (toBeUpdated, toBeRemoved, toBeSent);
    }

    private async Task<IList<MastodonUserAccount>> _GetToSyncMastodonUserAccounts(Note note)
    {
        var result = new List<MastodonUserAccount>();
        var all = await mastodonUserAccountCacheService.GetAsync(note.UserId);
        foreach (var account in all)
        {
            switch (account.SyncType)
            {
                case MastodonSyncType.All:
                    result.Add(account);
                    break;
                case MastodonSyncType.PublicOnly:
                    if (!note.IsPrivate)
                    {
                        result.Add(account);
                    }

                    break;
                case MastodonSyncType.TagMastodonOnly:
                    if (note.TagList.Contains("mastodon"))
                    {
                        result.Add(account);
                    }

                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets instances that require note updates.
    /// </summary>
    private List<MastodonSyncedInstance> _getInstancesToBeUpdated(List<MastodonSyncedInstance> instances,
        IList<MastodonUserAccount> toSyncInstances)
    {
        var idsToUpdate = instances.Select(i => i.UserAccountId).Intersect(toSyncInstances.Select(t => t.Id)).ToList();
        return instances.Where(r => idsToUpdate.Contains(r.UserAccountId)).ToList();
    }

    /// <summary>
    /// Gets instances where synced notes need to be removed.
    /// </summary>
    private List<MastodonSyncedInstance> _GetInstancesToBeRemoved(List<MastodonSyncedInstance> instances,
        IList<MastodonUserAccount> toSyncAccounts)
    {
        if (!toSyncAccounts.Any()) return instances;

        var idsToRemove = instances.Select(s => s.UserAccountId).Except(toSyncAccounts.Select(t => t.Id)).ToList();
        return instances.Where(r => idsToRemove.Contains(r.UserAccountId)).ToList();
    }

    /// <summary>
    /// Gets mastodon user accounts that need new note synchronization.
    /// </summary>
    private List<MastodonUserAccount> _GetAccountsToBeSent(List<MastodonSyncedInstance> syncedInstances,
        IList<MastodonUserAccount> toSyncAccounts)
    {
        if (!syncedInstances.Any()) return toSyncAccounts.ToList();
        var toSendUserAccountId = toSyncAccounts.Select(t => t.Id).Except(syncedInstances.Select(s => s.UserAccountId))
            .ToList();
        return toSyncAccounts.Where(t => toSendUserAccountId.Contains(t.Id)).ToList();
    }

    private async Task _DeleteToot(MastodonSyncedInstance instance, MastodonUserAccount account)
    {
        logger.LogDebug("Deleting toot {TootId} from account {AccountId} ({InstanceUrl})",
            instance.TootId, account.Id, account.InstanceUrl);

        try
        {
            await mastodonTootService.DeleteTootAsync(account.InstanceUrl,
                account.DecryptedAccessToken(TokenKey), instance.TootId);

            logger.LogDebug("Successfully deleted toot {TootId} from {InstanceUrl}",
                instance.TootId, account.InstanceUrl);
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex, "ApiRequestException while deleting toot {TootId} from {InstanceUrl}: {ErrorMessage}",
                instance.TootId, account.InstanceUrl, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete toot {TootId} from {InstanceUrl}: {ErrorMessage}",
                instance.TootId, account.InstanceUrl, ex.Message);
        }
    }

    private static List<MastodonSyncedInstance> _GetSyncedInstances(Note note)
    {
        if (string.IsNullOrWhiteSpace(note.MastodonTootIds)) return [];
        return note.MastodonTootIds.Split(",").Select(s =>
        {
            var sync = s.Split(":");
            return new MastodonSyncedInstance
            {
                UserAccountId = long.Parse(sync[0]),
                TootId = sync[1],
            };
        }).ToList();
    }
}
