using Api.Framework.Helper;
using Api.Framework.Models;
using HappyNotes.Common;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using HappyNotes.Services.SyncQueue.Interfaces;
using HappyNotes.Services.SyncQueue.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;

namespace HappyNotes.Services;

public class TelegramSyncNoteService(
    ITelegramService telegramService,
    ITelegramSettingsCacheService telegramSettingsCacheService,
    INoteRepository noteRepository,
    IOptions<JwtConfig> jwtConfig,
    ISyncQueueService syncQueueService,
    ILogger<TelegramSyncNoteService> logger
)
    : ISyncNoteService
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;
    private string TokenKey => _jwtConfig.SymmetricSecurityKey;

    public async Task SyncNewNote(Note note, string fullContent)
    {
        try
        {
            var syncedChannels = new List<SyncedTelegramChannel>();
            var validSettings = await telegramSettingsCacheService.GetAsync(note.UserId);
            if (validSettings.Any())
            {
                var toSyncChannelIds = (await _GetRequiredChannelData(note)).toBeSent;
                if (toSyncChannelIds.Any())
                {
                    var token = TextEncryptionHelper.Decrypt(validSettings.First().EncryptedToken,
                        TokenKey);

                    // Try immediate sync first, fallback to queue on failure
                    foreach (var channelId in toSyncChannelIds)
                    {
                        try
                        {
                            var messageId = await _SentNoteToChannel(note, fullContent, token, channelId);
                            syncedChannels.Add(new SyncedTelegramChannel
                            {
                                ChannelId = channelId,
                                MessageId = messageId,
                            });

                            logger.LogDebug("Successfully synced note {NoteId} to Telegram channel {ChannelId}",
                                note.Id, channelId);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to sync note {NoteId} to channel {ChannelId}, queuing for retry",
                                note.Id, channelId);

                            // Queue the task for retry
                            await EnqueueSyncTask(note, fullContent, token, channelId, "CREATE");
                        }
                    }
                }
            }

            note.UpdateTelegramMessageIds(syncedChannels);
            await noteRepository.UpdateAsync(note);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in SyncNewNote for note {NoteId}", note.Id);
        }
    }

    public async Task SyncEditNote(Note note, string fullContent)
    {
        var settings = await telegramSettingsCacheService.GetAsync(note.UserId);
        if (!settings.Any()) return;

        var syncedChannels = new List<SyncedTelegramChannel>();

        var channelData = await _GetRequiredChannelData(note);
        var toBeSent = channelData.toBeSent;

        // there are existing synced channels
        if (!string.IsNullOrWhiteSpace(note.TelegramMessageIds))
        {
            var tobeRemoved = channelData.toBeRemovedChannels;
            foreach (var channel in tobeRemoved)
            {
                var setting = settings.FirstOrDefault(s => s.ChannelId.Equals(channel.ChannelId));
                if (setting != null) await _DeleteMessage(setting, channel);
            }

            var tobeUpdated = channelData.toBeUpdatedChannels;

            // First, handle messages that are short enough to be edited in-place.
            if (fullContent.Length <= Constants.TelegramMessageLength)
            {
                var successfullyEdited = new List<SyncedTelegramChannel>();
                try
                {
                    foreach (var channel in tobeUpdated)
                    {
                        var setting = settings.FirstOrDefault(s => s.ChannelId.Equals(channel.ChannelId));
                        if (setting == null) continue;

                        var token = TextEncryptionHelper.Decrypt(setting.EncryptedToken, TokenKey);
                        try
                        {
                            await telegramService.EditMessageAsync(token, channel.ChannelId, channel.MessageId, fullContent, note.IsMarkdown);
                        }
                        catch (ApiRequestException ex)
                        {
                            logger.LogError(ex, "Failed to edit message with Markdown for note {NoteId} in channel {ChannelId}. Retrying without Markdown.", note.Id, channel.ChannelId);
                            if (note.IsMarkdown)
                            {
                                await telegramService.EditMessageAsync(token, channel.ChannelId, channel.MessageId, fullContent, false);
                            }
                        }

                        // This message is now successfully updated.
                        syncedChannels.Add(new SyncedTelegramChannel
                        {
                            ChannelId = channel.ChannelId,
                            MessageId = channel.MessageId,
                        });
                        successfullyEdited.Add(channel);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An unexpected error occurred while editing messages for note {NoteId}", note.Id);
                }
                // Remove the successfully edited channels from the list of channels to be updated.
                tobeUpdated.RemoveAll(c => successfullyEdited.Contains(c));
            }

            // For messages that were too long (or if editing failed), delete and resend.
            foreach (var channel in tobeUpdated)
            {
                var setting = settings.FirstOrDefault(s => s.ChannelId.Equals(channel.ChannelId));
                if (setting != null)
                {
                    await _DeleteMessage(setting, channel);
                    toBeSent.Add(channel.ChannelId);
                }
            }
        }

        // This part remains the same
        foreach (var channelId in toBeSent)
        {
            var setting = settings.FirstOrDefault(s => s.ChannelId.Equals(channelId));
            if (setting == null) continue;

            var token = TextEncryptionHelper.Decrypt(setting.EncryptedToken,
                TokenKey);
            var messageId = await _SentNoteToChannel(note, fullContent, token, channelId);
            syncedChannels.Add(new SyncedTelegramChannel
            {
                ChannelId = channelId,
                MessageId = messageId,
            });
        }

        note.UpdateTelegramMessageIds(syncedChannels);
        await noteRepository.UpdateAsync(_ => new Note
        {
            TelegramMessageIds = note.TelegramMessageIds,
        }, where => where.Id == note.Id);
    }

    public async Task SyncDeleteNote(Note note)
    {
        if (!string.IsNullOrWhiteSpace(note.TelegramMessageIds))
        {
            try
            {
                var settings = await telegramSettingsCacheService.GetAsync(note.UserId);
                if (!settings.Any()) return;

                var syncedChannels = _GetSyncedChannels(note);
                foreach (var channel in syncedChannels)
                {
                    var channelId = channel.ChannelId;
                    var setting = settings.FirstOrDefault(s => s.ChannelId.Equals(channelId));
                    if (setting == null) continue;

                    // Use queue instead of direct deletion - content not needed for DELETE
                    await EnqueueSyncTask(note, string.Empty, string.Empty, channelId, "DELETE", channel.MessageId);
                }

                // Note: TelegramMessageIds will be cleared by the handler after successful deletion
                // This ensures we can retry if the deletion fails
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error enqueueing Telegram delete tasks for note {NoteId}", note.Id);
            }
        }
    }

    public async Task SyncUndeleteNote(Note note)
    {
        // As per user instruction, no action is taken for undeleting notes on Telegram
        await Task.CompletedTask;
    }

    public async Task PurgeDeletedNotes()
    {
        // As per user instruction, no action is taken for purging deleted notes on Telegram
        await Task.CompletedTask;
    }

    /// <summary>
    /// Obtain three groups of channel data:
    /// 1. toBeUpdatedChannels: Channels that need note updates.
    /// 2. toBeRemovedChannels: Channels where synced notes need to be removed.
    /// 3. toBeSent: Channel IDs that need new note synchronization.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    private async Task<(List<SyncedTelegramChannel> toBeUpdatedChannels,
        List<SyncedTelegramChannel> toBeRemovedChannels,
        List<string> toBeSent)> _GetRequiredChannelData(Note note)
    {
        var syncedChannels = _GetSyncedChannels(note);
        var toSyncChannelIds = _GetToSyncChannelIds(note, await telegramSettingsCacheService.GetAsync(note.UserId));

        var toBeUpdated = _GetChannelsToBeUpdated(syncedChannels, toSyncChannelIds);
        var toBeRemoved = _GetChannelsToBeRemoved(syncedChannels, toSyncChannelIds);
        var toBeSentIds = _GetChannelsToBeSent(syncedChannels, toSyncChannelIds);

        return (toBeUpdated, toBeRemoved, toBeSentIds);
    }

    /// <summary>
    /// Gets channels that require note updates.
    /// </summary>
    private List<SyncedTelegramChannel> _GetChannelsToBeUpdated(List<SyncedTelegramChannel> syncedChannels,
        List<string> toSyncChannelIds)
    {
        var idsToUpdate = syncedChannels.Select(s => s.ChannelId).Intersect(toSyncChannelIds).ToList();
        return syncedChannels.Where(r => idsToUpdate.Contains(r.ChannelId)).ToList();
    }

    /// <summary>
    /// Gets channels where synced notes need to be removed.
    /// </summary>
    private List<SyncedTelegramChannel> _GetChannelsToBeRemoved(List<SyncedTelegramChannel> syncedChannels,
        List<string> toSyncChannelIds)
    {
        if (!toSyncChannelIds.Any()) return syncedChannels;

        var idsToRemove = syncedChannels.Select(s => s.ChannelId).Except(toSyncChannelIds).ToList();
        return syncedChannels.Where(r => idsToRemove.Contains(r.ChannelId)).ToList();
    }

    /// <summary>
    /// Gets channel IDs that need new note synchronization.
    /// </summary>
    private List<string> _GetChannelsToBeSent(List<SyncedTelegramChannel> syncedChannels, List<string> toSyncChannelIds)
    {
        if (!syncedChannels.Any()) return toSyncChannelIds;
        return toSyncChannelIds.Except(syncedChannels.Select(s => s.ChannelId)).ToList();
    }

    private async Task<int> _SentNoteToChannel(Note note, string fullContent, string token, string channelId)
    {
        var text = fullContent; // Or format the message as needed

        // You can use different logic here based on the note's properties
        if (fullContent.Length > Constants.TelegramMessageLength)
        {
            var message = await telegramService.SendLongMessageAsFileAsync(token, channelId, fullContent,
                note.IsMarkdown ? ".md" : ".txt");
            return message.MessageId;
        }
        else
        {
            try
            {
                var message = await telegramService.SendMessageAsync(token, channelId, text, note.IsMarkdown);
                return message.MessageId;
            }
            catch (ApiRequestException ex)
            {
                logger.LogError(ex, "Failed to send message with Markdown for note {NoteId} in channel {ChannelId}. Retrying without Markdown.", note.Id, channelId);
                if (note.IsMarkdown)
                {
                    // Fallback: retry sending without markdown
                    var message = await telegramService.SendMessageAsync(token, channelId, text, false);
                    return message.MessageId;
                }
                throw; // Re-throw if it wasn't a markdown issue or if we shouldn't retry
            }
        }
    }

    private List<string> _GetToSyncChannelIds(Note note, IList<TelegramSettings> settings)
    {
        var targetChannelList = new List<string>();
        foreach (var setting in settings)
        {
            switch (setting.SyncType)
            {
                case TelegramSyncType.All:
                    targetChannelList.Add(setting.ChannelId);
                    break;
                case TelegramSyncType.Private:
                    if (note.IsPrivate)
                    {
                        targetChannelList.Add(setting.ChannelId);
                    }

                    break;
                case TelegramSyncType.Public:
                    if (!note.IsPrivate)
                    {
                        targetChannelList.Add(setting.ChannelId);
                    }

                    break;
                case TelegramSyncType.Tag:
                    if (note.TagList.Contains(setting.SyncValue))
                    {
                        targetChannelList.Add(setting.ChannelId);
                    }

                    break;
            }
        }

        return targetChannelList.Distinct().ToList();
    }

    private async Task _DeleteMessage(TelegramSettings setting, SyncedTelegramChannel telegramChannel)
    {
        var token = TextEncryptionHelper.Decrypt(setting.EncryptedToken, TokenKey);
        try
        {
            await telegramService.DeleteMessageAsync(token, telegramChannel.ChannelId, telegramChannel.MessageId);
        }
        catch (ApiRequestException ex)
        {
            logger.LogError(ex.ToString());
        }
    }

    private static List<SyncedTelegramChannel> _GetSyncedChannels(Note note)
    {
        if (string.IsNullOrWhiteSpace(note.TelegramMessageIds)) return [];
        return note.TelegramMessageIds.Split(",").Select(s =>
        {
            var sync = s.Split(":");
            return new SyncedTelegramChannel
            {
                ChannelId = sync[0],
                MessageId = int.TryParse(sync[1], out var messageId) ? messageId : 0,
            };
        }).ToList();
    }

    private async Task EnqueueSyncTask(Note note, string fullContent, string token, string channelId, string action, int? messageId = null)
    {
        var payload = new TelegramSyncPayload
        {
            Action = action,
            FullContent = fullContent,
            // Security: BotToken removed - will be retrieved in handler using UserId + ChannelId
            ChannelId = channelId,
            MessageId = messageId,
            IsMarkdown = note.IsMarkdown
        };

        var task = SyncTask.Create("telegram", action, note.Id, note.UserId, payload);

        await syncQueueService.EnqueueAsync("telegram", task);

        logger.LogInformation("Enqueued Telegram sync task {TaskId} for note {NoteId}, action: {Action}",
            task.Id, note.Id, action);
    }
}
