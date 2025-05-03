using Api.Framework.Helper;
using Api.Framework.Models;
using HappyNotes.Common;
using HappyNotes.Common.Enums;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;

namespace HappyNotes.Services;

public class TelegramSyncNoteService(
    ITelegramService telegramService,
    ITelegramSettingsCacheService telegramSettingsCacheService,
    INoteRepository noteRepository,
    IOptions<JwtConfig> jwtConfig,
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
                    foreach (var channelId in toSyncChannelIds)
                    {
                        var messageId = await _SentNoteToChannel(note, fullContent, token, channelId);
                        syncedChannels.Add(new SyncedTelegramChannel
                        {
                            ChannelId = channelId,
                            MessageId = messageId,
                        });
                    }
                }
            }

            note.UpdateTelegramMessageIds(syncedChannels);
            await noteRepository.UpdateAsync(note);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
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
            if (fullContent.Length <= Constants.TelegramMessageLength)
            {
                try
                {
                    foreach (var channel in tobeUpdated)
                    {
                        var channelId = channel.ChannelId;
                        var setting = settings.FirstOrDefault(s => s.ChannelId.Equals(channelId));
                        if (setting == null) continue;

                        var token = TextEncryptionHelper.Decrypt(setting.EncryptedToken,
                            TokenKey);
                        try
                        {
                            await telegramService.EditMessageAsync(token, channelId, channel.MessageId, fullContent,
                                note.IsMarkdown);
                        }
                        catch (ApiRequestException ex)
                        {
                            logger.LogError(ex.ToString());
                            if (note.IsMarkdown)
                            {
                                // exception can be caused by the Markdown parser, so we do the edit again without setting markdown flag
                                await telegramService.EditMessageAsync(token, channelId, channel.MessageId, fullContent,
                                    false);
                            }
                        }
                        finally
                        {
                            syncedChannels.Add(new SyncedTelegramChannel
                            {
                                ChannelId = channelId,
                                MessageId = channel.MessageId,
                            });
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
                    var setting = settings.FirstOrDefault(s => s.ChannelId.Equals(channel.ChannelId));
                    if (setting != null)
                    {
                        await _DeleteMessage(setting, channel);
                        toBeSent.Add(channel.ChannelId);
                    }
                }
            }
        }

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

                    await _DeleteMessage(setting, channel);
                }

                note.TelegramMessageIds = null;
                await noteRepository.UpdateAsync(note);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
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
            var message = await telegramService.SendMessageAsync(token, channelId, text, note.IsMarkdown);
            return message.MessageId;
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
}
