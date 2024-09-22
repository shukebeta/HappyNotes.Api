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
using EventId = HappyNotes.Common.Enums.EventId;

namespace HappyNotes.Services;

public class NoteService(
    ILogger<NoteService> logger,
    INoteRepository noteRepository,
    INoteTagService noteTagService,
    IRepositoryBase<LongNote> longNoteRepository,
    IRepositoryBase<TelegramSettings> telegramSettingsRepository,
    ITelegramService telegramService,
    IOptions<JwtConfig> jwtConfig,
    CurrentUser currentUser
) : INoteService
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;

    public async Task<long> Post(PostNoteRequest request)
    {
        var fullContent = request.Content?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(fullContent) && (request.Attachments == null || !request.Attachments.Any()))
        {
            throw new ArgumentException("Nothing was submitted");
        }

        var tagList = fullContent.GetTags();
        var note = new Note
        {
            UserId = currentUser.Id,
            IsPrivate = request.IsPrivate,
            IsMarkdown = request.IsMarkdown,
            CreatedAt = DateTime.UtcNow.ToUnixTimeSeconds(),
            IsLong = fullContent.IsLong(),
            Tags = string.Join(' ', tagList),
            TagList = tagList,
            Content = fullContent.IsLong() ? fullContent.GetShort() : fullContent
        };

        await noteRepository.InsertAsync(note);

        if (note.IsLong)
        {
            await longNoteRepository.InsertAsync(new LongNote
            {
                Id = note.Id,
                Content = fullContent
            });
        }

        try
        {
            await _UpdateNoteTags(note, fullContent);
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            // ate the exception to avoid interrupting the main process
        }

        // Sync to Telegram asynchronously
        Task.Run(async () => await _SentToTelegram(note, fullContent));

        return note.Id;
    }

    private async Task _SentToTelegram(Note note, string fullContent)
    {
        try
        {
            var validSettings = await _GetValidTelegramSettings(note);
            if (!validSettings.Any()) return;

            var toSyncChannelIds = _GetRequiredSyncChannelIds(note, validSettings);
            if (!toSyncChannelIds.Any()) return;

            var token = TextEncryptionHelper.Decrypt(validSettings.First().EncryptedToken,
                _jwtConfig.SymmetricSecurityKey);
            foreach (var channelId in toSyncChannelIds)
            {
                await _SentNoteToChannel(note, fullContent, token, channelId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
        }
    }

    /// <summary>
    /// Obtain what channel IDs require the removal of this note
    /// </summary>
    /// <param name="note"></param>
    /// <param name="validSettings"></param>
    /// <returns></returns>
    private async Task<List<SyncedChannel>> _GetRequiredRemovalChannelIds(Note note, TelegramSettings[]? validSettings)
    {
        var syncedChannels = _GetSyncedChannels(note);

        // If existing channels are empty, nothing to delete
        if (!syncedChannels.Any()) return [];

        var toSyncChannelIds = _GetToSyncChannelIds(note, validSettings ?? await _GetValidTelegramSettings(note));

        // If no channels need to sync, return all existing channels
        if (!toSyncChannelIds.Any()) return syncedChannels;

        // Return channels that are synced but not in the to-sync list
        var ids = syncedChannels.Select(s => s.ChannelId).Except(toSyncChannelIds).ToList();
        return syncedChannels.Where(r => ids.Contains(r.ChannelId)).ToList();
    }

    private async Task _DeleteTelegramMessageIfNeeded(Note note, TelegramSettings[] validSettings)
    {
        var toRemoveChannels = await _GetRequiredRemovalChannelIds(note, validSettings);
        if (!toRemoveChannels.Any()) return;
        var settings = await telegramSettingsRepository.GetListAsync(
            s => s.UserId == note.UserId && s.Status == TelegramSettingStatus.Normal);

        if (!settings.Any()) return;
        foreach (var channel in toRemoveChannels)
        {
            try
            {
                var setting = settings.FirstOrDefault(s => s.ChannelId.Equals(channel.ChannelId));
                if (setting != null) await _DeleteMessage(setting, channel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }

    }

    /// <summary>
    /// Obtain the channel ids that truly need to sync
    /// </summary>
    /// <param name="note"></param>
    /// <param name="validSettings"></param>
    /// <returns></returns>
    private List<string> _GetRequiredSyncChannelIds(Note note, TelegramSettings[] validSettings)
    {
        var toSyncChannelIds = _GetToSyncChannelIds(note, validSettings);
        if (!toSyncChannelIds.Any()) return toSyncChannelIds;
        var syncedChannelIds = _GetSyncedChannels(note);
        if (toSyncChannelIds.Any())
        {
            toSyncChannelIds = toSyncChannelIds.Except(syncedChannelIds.Select(s => s.ChannelId)).ToList();
        }

        return toSyncChannelIds;
    }

    private async Task<TelegramSettings[]> _GetValidTelegramSettings(Note note)
    {
        var telegramSettings = await telegramSettingsRepository.GetListAsync(x => x.UserId == note.UserId);
        if (!telegramSettings.Any()) return [];
        var validSettings = telegramSettings.Where(s => s.Status.Is(TelegramSettingStatus.Normal)).ToArray();
        return validSettings;
    }

    private async Task _SentNoteToChannel(Note note, string fullContent, string token, string channelId)
    {
        var text = fullContent; // Or format the message as needed

        // You can use different logic here based on the note's properties
        if (fullContent.Length > Constants.TelegramMessageLength)
        {
            await telegramService.SendLongMessageAsFileAsync(token, channelId, fullContent,
                note.IsMarkdown ? ".md" : ".txt");
        }
        else
        {
            var message = await telegramService.SendMessageAsync(token, channelId, text, note.IsMarkdown);
            if (message.MessageId > 0)
            {
                note.UpsertTelegramMessageIdList(channelId, message.MessageId);
                await noteRepository.UpdateAsync(note);
            }
        }
    }

    private List<string> _GetToSyncChannelIds(Note note, TelegramSettings[] settings)
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

    public async Task<bool> Update(long id, PostNoteRequest request)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note == null)
        {
            throw ExceptionHelper.New(id, EventId._00100_NoteNotFound, id);
        }

        if (_NoteIsNotYours(note))
        {
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        var fullContent = request.Content?.Trim() ?? string.Empty;
        await _UpdateNoteTags(note, fullContent);

        note.IsPrivate = request.IsPrivate;
        note.IsMarkdown = request.IsMarkdown;
        note.UpdatedAt = DateTime.UtcNow.ToUnixTimeSeconds();
        note.IsLong = fullContent.IsLong();
        var tagList = fullContent.GetTags();
        note.TagList = tagList;
        note.Tags = string.Join(' ', tagList);

        if (note.IsLong)
        {
            var longNote = new LongNote {Id = id, Content = fullContent};
            await longNoteRepository.UpsertAsync(longNote, n => n.Id == id);
        }

        note.Content = fullContent.GetShort();
        Task.Run(async () => await _UpdateTelegramMessageAsync(note, fullContent));
        return await noteRepository.UpdateAsync(note);
    }

    private async Task _UpdateTelegramMessageAsync(Note note, string fullContent)
    {
        if (!string.IsNullOrWhiteSpace(note.TelegramMessageIds) &&
            fullContent.Length <= Constants.TelegramMessageLength)
        {
            try
            {
                var settings = await telegramSettingsRepository.GetListAsync(
                    s => s.UserId == note.UserId && s.Status == TelegramSettingStatus.Normal);

                if (!settings.Any()) return;

                var syncChannels = _GetSyncedChannels(note);
                foreach (var channel in syncChannels)
                {
                    var channelId = channel.ChannelId;
                    var setting = settings.FirstOrDefault(s => s.ChannelId.Equals(channelId));
                    if (setting == null) continue;

                    var token = TextEncryptionHelper.Decrypt(setting.EncryptedToken,
                        _jwtConfig.SymmetricSecurityKey);
                    try
                    {
                        await telegramService.EditMessageAsync(token, channelId, channel.MessageId, fullContent,
                            note.IsMarkdown);

                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                    {
                        logger.LogError(ex.ToString());
                        if (note.IsMarkdown)
                        {
                            // exception can be caused by the Markdown parser, so we do the edit again without setting markdown flag
                            await telegramService.EditMessageAsync(token, channelId, channel.MessageId, fullContent, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }

    private async Task _DeleteTelegramMessageAsync(Note note)
    {
        if (!string.IsNullOrWhiteSpace(note.TelegramMessageIds))
        {
            try
            {
                var settings = await telegramSettingsRepository.GetListAsync(
                    s => s.UserId == note.UserId && s.Status == TelegramSettingStatus.Normal);

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

    private async Task _DeleteMessage(TelegramSettings setting, SyncedChannel channel)
    {
        var token = TextEncryptionHelper.Decrypt(setting.EncryptedToken,
            _jwtConfig.SymmetricSecurityKey);
        try
        {
            await telegramService.DeleteMessageAsync(token, channel.ChannelId, channel.MessageId);
        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex)
        {
            logger.LogError(ex.ToString());
        }
    }

    private static List<SyncedChannel> _GetSyncedChannels(Note note)
    {
        return note.TelegramMessageIds?.Split(",").Select(s =>
        {
            var sync = s.Split(":");
            return new SyncedChannel()
            {
                ChannelId = sync[0],
                MessageId = int.TryParse(sync[1], out var messageId) ? messageId : 0,
            };
        }).ToList() ?? [];
    }

    private async Task _UpdateNoteTags(Note note, string fullContent)
    {
        var oldTags = (string.IsNullOrWhiteSpace(note.Tags)) ? [] : note.Tags.Split(" ").ToList();
        var tags = fullContent.GetTags();
        if (oldTags.Any())
        {
            var toDelete = oldTags.Except(tags).ToList();
            if (toDelete.Any())
            {
                await noteTagService.Delete(note.Id, toDelete);
                Task.Run(async () => await _DeleteTelegramMessageAsync(note));
            }
        }

        if (tags.Any())
        {
            await noteTagService.Upsert(note, tags);
        }
    }

    public async Task<PageData<Note>> GetUserNotes(long userId, int pageSize, int pageNumber,
        bool includePrivate = false)
    {
        return await noteRepository.GetUserNotes(userId, pageSize, pageNumber, includePrivate);
    }

    public async Task<PageData<Note>> GetPublicNotes(int pageSize, int pageNumber)
    {
        if (pageNumber > Constants.PublicNotesMaxPage)
        {
            throw new Exception(
                $"We only provide at most {Constants.PublicNotesMaxPage} page of public notes at the moment");
        }

        var notes = await noteRepository.GetPublicNotes(pageSize, pageNumber);
        if (notes.TotalCount > Constants.PublicNotesMaxPage * pageSize)
        {
            notes.TotalCount = Constants.PublicNotesMaxPage * pageSize;
        }

        return notes;
    }

    public async Task<PageData<Note>> GetUserTagNotes(long userId, int pageSize, int pageNumber, string tag)
    {
        return await noteRepository.GetUserTagNotes(userId, tag, pageSize, pageNumber);
    }

    public async Task<PageData<Note>> GetLinkedNotes(long userId, long noteId)
    {
        return await noteRepository.GetLinkedNotes(userId, noteId);
    }

    /// <summary>
    /// Providing memories like
    /// - 5 years ago, [what happened today]
    /// - 4 years ago...
    /// </summary>
    /// <param name="localTimezone"></param>
    /// <returns></returns>
    public async Task<IList<Note>> Memories(string localTimezone)
    {
        var earliest = await noteRepository.GetFirstOrDefaultAsync(w => w.UserId.Equals(currentUser.Id));
        if (earliest == null) return [];
        var notes = new List<Note>();
        var periodList = _GetTimestamps(earliest.CreatedAt, localTimezone);
        foreach (var start in periodList)
        {
            var note = await noteRepository.GetFirstOrDefaultAsync(w =>
                w.UserId.Equals(currentUser.Id) && w.CreatedAt >= start && w.CreatedAt < start + 86400 &&
                w.DeletedAt == null);
            if (note != null)
            {
                notes.Add(note);
            }
        }

        return notes;
    }

    public async Task<IList<Note>> MemoriesOn(string localTimezone, string yyyyMMdd)
    {
        // Get the specified time zone
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimezone);
        // Parse the date string to a DateTime object
        var dayStartTimestamp = DateTimeOffset.ParseExact(yyyyMMdd, "yyyyMMdd", CultureInfo.InvariantCulture)
            .GetDayStartTimestamp(timeZone);
        var notes = await noteRepository.GetListAsync(w =>
            w.UserId.Equals(currentUser.Id) && w.CreatedAt >= dayStartTimestamp &&
            w.CreatedAt < dayStartTimestamp + 86400 && w.DeletedAt == null);
        return (notes);
    }

    private static long[] _GetTimestamps(long initialUnixTimestamp, string timeZoneId)
    {
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var startDateTimeOffset = initialUnixTimestamp.ToDateTimeOffset(targetTimeZone);

        var startYear = startDateTimeOffset.Year;
        var startMonth = startDateTimeOffset.Month;
        var startDay = startDateTimeOffset.Day;

        var nowInTargetTimeZone = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, targetTimeZone);
        var todayStartTimeOffset = new DateTimeOffset(nowInTargetTimeZone.Year, nowInTargetTimeZone.Month,
            nowInTargetTimeZone.Day, 0, 0, 0, nowInTargetTimeZone.Offset);

        var currentYear = nowInTargetTimeZone.Year;
        var currentMonth = nowInTargetTimeZone.Month;
        var currentDay = nowInTargetTimeZone.Day;

        var timestamps = new List<long>();

        if (startYear < currentYear)
        {
            for (var year = startMonth <= currentMonth && startDay <= currentDay
                     ? startYear
                     : startYear + 1;
                 year < currentYear;
                 year++)
            {
                try
                {
                    var targetDate = new DateTimeOffset(year, currentMonth, currentDay, 0, 0, 0,
                        nowInTargetTimeZone.Offset);
                    timestamps.Add(targetDate.ToUnixTimeSeconds());
                }
                catch (ArgumentOutOfRangeException)
                {
                    // skip invalid dates，such as 02-30, 02-31
                    // do nothing
                }
            }
        }

        if (startDateTimeOffset.AddMonths(6).EarlierThan(nowInTargetTimeZone))
        {
            timestamps.Add(todayStartTimeOffset.AddMonths(-6).ToUnixTimeSeconds());
        }

        if (startDateTimeOffset.AddMonths(3).EarlierThan(nowInTargetTimeZone))
        {
            timestamps.Add(todayStartTimeOffset.AddMonths(-3).ToUnixTimeSeconds());
        }

        if (startDateTimeOffset.AddMonths(1).EarlierThan(nowInTargetTimeZone))
        {
            timestamps.Add(todayStartTimeOffset.AddMonths(-1).ToUnixTimeSeconds());
        }

        timestamps.Add(todayStartTimeOffset.AddDays(-1).ToUnixTimeSeconds());
        timestamps.Add(todayStartTimeOffset.ToUnixTimeSeconds());
        return timestamps.ToArray();
    }

    public async Task<Note> Get(long noteId)
    {
        var note = await noteRepository.Get(noteId);
        if (note is null)
        {
            throw ExceptionHelper.New(noteId, EventId._00100_NoteNotFound, noteId);
        }

        if (note.IsPrivate && _NoteIsNotYours(note))
        {
            throw ExceptionHelper.New(noteId, EventId._00101_NoteIsPrivate, noteId);
        }

        return note;
    }

    private bool _NoteIsNotYours(Note note)
    {
        return currentUser.Id != note.UserId;
    }

    public async Task<bool> Delete(long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note == null)
        {
            throw new Exception($"Note with Id: {id} does not exist");
        }

        if (_NoteIsNotYours(note))
        {
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        // already deleted before
        if (note.DeletedAt is not null)
        {
            return true;
        }

        note.DeletedAt = DateTime.UtcNow.ToUnixTimeSeconds();
        Task.Run(async () => await _DeleteTelegramMessageAsync(note));
        return await noteRepository.UpdateAsync(note);
    }

    public async Task<bool> Undelete(long id)
    {
        var note = await noteRepository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (note is null)
        {
            throw ExceptionHelper.New(id, EventId._00100_NoteNotFound, id);
        }

        if (note.DeletedAt is null)
        {
            throw ExceptionHelper.New(id, EventId._00103_NoteIsNotDeleted, id);
        }

        if (_NoteIsNotYours(note))
        {
            throw ExceptionHelper.New(id, EventId._00102_NoteIsNotYours, id);
        }

        note.DeletedAt = null;
        return await noteRepository.UpdateAsync(note);
    }
}
