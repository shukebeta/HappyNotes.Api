using System.Globalization;
using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Models;
using HappyNotes.Common;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Extensions;
using HappyNotes.Models;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeihanLi.Extensions;
using EventId = HappyNotes.Common.Enums.EventId;

namespace HappyNotes.Services;

public partial class NoteService
{
    /// <summary>
    /// new note: send to Mastodon
    /// </summary>
    /// <param name="note"></param>
    /// <param name="fullContent"></param>
    private async Task _NewNoteSendToMastodon(Note note, string fullContent)
    {
        var syncedInstances = new List<MastodonSyncedInstance>();
        try
        {
            var accounts = await mastodonUserAccountCacheService.GetAsync(note.UserId);
            if (accounts.Any())
            {
                foreach (var account in accounts)
                {
                    var tootId = await _SentNoteToMastodon(note, fullContent, account);
                    syncedInstances.Add(new MastodonSyncedInstance()
                    {
                        UserAccountId = account.Id,
                        TootId = tootId,
                    });
                }
            }

            note.UpdateMastodonInstanceIds(syncedInstances);
            await noteRepository.UpdateAsync(note);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }
    }

    private async Task<string> _SentNoteToMastodon(Note note, string fullContent, MastodonUserAccount account)
    {
        var text = fullContent; // Or format the message as needed

        // You can use different logic here based on the note's properties
        if (fullContent.Length > Constants.MastodonTootLength)
        {
            // todo, skip
            return string.Empty;
        }
        else
        {
            var toot = await mastodonTootService.SendTootAsync(account.InstanceUrl,
                account.DecryptedAccessToken(_jwtConfig.SymmetricSecurityKey), text, note.IsMarkdown);
            return toot.Id;
        }
    }

    private async Task _UpdateTootAsync(Note note, string fullContent)
    {
        var accounts = await mastodonUserAccountCacheService.GetAsync(note.UserId);
        if (!accounts.Any()) return;
        var instances = new List<MastodonSyncedInstance>();

        var instanceData = await _GetInstancesData(note);
        var toBeSent = instanceData.toBeSent;
        // there are existing synced channels
        if (!string.IsNullOrWhiteSpace(note.MastodonTootIds))
        {
            var tobeRemoved = instanceData.toBeRemoved;
            foreach (var instance in tobeRemoved)
            {
                var account = accounts.FirstOrDefault(s => s.Id.Equals(instance.UserAccountId));
                if (account != null) await _DeleteToot(instance, account);
            }

            var tobeUpdated = instanceData.toBeUpdated;
            if (fullContent.Length <= Constants.MastodonTootLength)
            {
                try
                {
                    foreach (var instance in tobeUpdated)
                    {
                        var userAccountId = instance.UserAccountId;
                        var account = accounts.FirstOrDefault(s => s.Id.Equals(userAccountId));
                        if (account == null) continue;

                        try
                        {
                            await mastodonTootService.EditTootAsync(account.InstanceUrl, account.DecryptedAccessToken(_jwtConfig.SymmetricSecurityKey), instance.TootId, fullContent,
                                note.IsPrivate);
                            instances.Add(new MastodonSyncedInstance()
                            {
                                UserAccountId = userAccountId,
                                TootId = instance.TootId,
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.ToString());
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                }
            }
            else
            {
                // delete existing message, then resend as message with attachments
                foreach (var channel in tobeUpdated)
                {
                    // todo, convert text to image, and edit the toot with image
                }
            }
        }

        foreach (var account in toBeSent)
        {
            var tootId = await _SentNoteToMastodon(note, fullContent, account);
            instances.Add(new MastodonSyncedInstance()
            {
                UserAccountId = account.Id,
                TootId = tootId,
            });
        }

        note.UpdateMastodonInstanceIds(instances);
        await noteRepository.UpdateAsync(note);
    }

    private async Task<(List<MastodonSyncedInstance> toBeUpdated, List<MastodonSyncedInstance> toBeRemoved, List<MastodonUserAccount> toBeSent)> _GetInstancesData(Note note)
    {
        var syncedChannels = _GetSyncedInstances(note);
        var toSyncInstances = await mastodonUserAccountCacheService.GetAsync(note.UserId);

        var toBeUpdated = _getInstancesToBeUpdated(syncedChannels, toSyncInstances);
        var toBeRemoved = _GetInstancesToBeRemoved(syncedChannels, toSyncInstances);
        var toBeSent = _GetAccountsToBeSent(syncedChannels, toSyncInstances);

        return (toBeUpdated, toBeRemoved, toBeSent);
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
    private List<MastodonUserAccount> _GetAccountsToBeSent(List<MastodonSyncedInstance> syncedInstances, IList<MastodonUserAccount> toSyncAccounts)
    {
        if (!syncedInstances.Any()) return toSyncAccounts.ToList();
        var toSendUserAccountId = toSyncAccounts.Select(t => t.Id).Except(syncedInstances.Select(s => s.UserAccountId)).ToList();
        return toSyncAccounts.Where(t => toSendUserAccountId.Contains(t.Id)).ToList();
    }


    private async Task _DeleteAllSyncedTootAsync(Note note)
    {
        if (!string.IsNullOrWhiteSpace(note.MastodonTootIds))
        {
            try
            {
                var accounts = await mastodonUserAccountCacheService.GetAsync(note.UserId);

                if (!accounts.Any()) return;

                var syncedInstances = _GetSyncedInstances(note);
                foreach (var instance in syncedInstances)
                {
                    var userAccountId = instance.UserAccountId;
                    var account = accounts.FirstOrDefault(s => s.Id.Equals(userAccountId));
                    if (account == null) continue;

                    await _DeleteToot(instance, account);
                }

                note.MastodonTootIds = null;
                await noteRepository.UpdateAsync(note);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }

    private async Task _DeleteToot(MastodonSyncedInstance instance, MastodonUserAccount account)
    {
        try
        {
            await mastodonTootService.DeleteTootAsync(account.InstanceUrl, account.DecryptedAccessToken(_jwtConfig.SymmetricSecurityKey), instance.TootId);
        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex)
        {
            logger.LogError(ex.ToString());
        }
    }

    private static List<MastodonSyncedInstance> _GetSyncedInstances(Note note)
    {
        if (string.IsNullOrWhiteSpace(note.MastodonTootIds)) return [];
        return note.MastodonTootIds.Split(",").Select(s =>
        {
            var sync = s.Split(":");
            return new MastodonSyncedInstance()
            {
                UserAccountId = long.Parse(sync[0]),
                TootId = sync[1],
            };
        }).ToList();
    }
}
