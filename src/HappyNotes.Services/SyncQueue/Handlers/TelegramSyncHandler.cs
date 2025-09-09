using System.Text.Json;
using Api.Framework.Helper;
using Api.Framework.Models;
using HappyNotes.Common;
using HappyNotes.Entities;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using HappyNotes.Services.SyncQueue.Configuration;
using HappyNotes.Services.SyncQueue.Interfaces;
using HappyNotes.Services.SyncQueue.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;

namespace HappyNotes.Services.SyncQueue.Handlers;

public class TelegramSyncHandler : ISyncHandler
{
    private readonly ITelegramService _telegramService;
    private readonly ITelegramSettingsCacheService _telegramSettingsCache;
    private readonly INoteRepository _noteRepository;
    private readonly SyncQueueOptions _options;
    private readonly JwtConfig _jwtConfig;
    private readonly ILogger<TelegramSyncHandler> _logger;

    public string ServiceName => "telegram";

    public int MaxRetryAttempts => _options.Handlers.TryGetValue(ServiceName, out var config)
        ? config.MaxRetries
        : 3;

    public TelegramSyncHandler(
        ITelegramService telegramService,
        ITelegramSettingsCacheService telegramSettingsCache,
        INoteRepository noteRepository,
        IOptions<SyncQueueOptions> options,
        IOptions<JwtConfig> jwtConfig,
        ILogger<TelegramSyncHandler> logger)
    {
        _telegramService = telegramService;
        _telegramSettingsCache = telegramSettingsCache;
        _noteRepository = noteRepository;
        _options = options.Value;
        _jwtConfig = jwtConfig.Value;
        _logger = logger;
    }

    public async Task<SyncResult> ProcessAsync(SyncTask task, CancellationToken cancellationToken)
    {
        try
        {
            TelegramSyncPayload? payload;

            // Handle both typed and untyped task objects
            if (task.Payload is JsonElement jsonElement)
            {
                payload = JsonSerializer.Deserialize<TelegramSyncPayload>(jsonElement.GetRawText(), JsonSerializerConfig.Default);
            }
            else if (task.Payload is TelegramSyncPayload telegramPayload)
            {
                payload = telegramPayload;
            }
            else
            {
                return SyncResult.Failure("Invalid payload type", shouldRetry: false);
            }

            if (payload == null)
            {
                return SyncResult.Failure("Failed to deserialize payload", shouldRetry: false);
            }

            // Security: Get BotToken from settings cache instead of payload
            var telegramSettings = await _telegramSettingsCache.GetAsync(task.UserId);
            var setting = telegramSettings.FirstOrDefault(s => s.ChannelId == payload.ChannelId);

            if (setting == null)
            {
                return SyncResult.Failure($"No Telegram settings found for user {task.UserId} and channel {payload.ChannelId}", shouldRetry: false);
            }

            _logger.LogDebug("Processing Telegram sync task {TaskId} - Action: {Action}, Channel: {ChannelId}",
                task.Id, payload.Action, payload.ChannelId);

            // Security: Decrypt token from encrypted storage
            var decryptedToken = TextEncryptionHelper.Decrypt(setting.EncryptedToken, _jwtConfig.SymmetricSecurityKey);

            await ProcessTelegramAction(payload, decryptedToken, task, cancellationToken);

            return SyncResult.Success();
        }
        catch (ApiRequestException ex) when (IsRetryableException(ex))
        {
            _logger.LogWarning("Retryable Telegram API error for task {TaskId}: {Error}", task.Id, ex.Message);
            return SyncResult.Failure(ex.Message, shouldRetry: true);
        }
        catch (ApiRequestException ex)
        {
            _logger.LogError("Non-retryable Telegram API error for task {TaskId}: {Error}", task.Id, ex.Message);
            return SyncResult.Failure(ex.Message, shouldRetry: false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("HTTP error for task {TaskId}: {Error}", task.Id, ex.Message);
            return SyncResult.Failure(ex.Message, shouldRetry: true);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Timeout error for task {TaskId}: {Error}", task.Id, ex.Message);
            return SyncResult.Failure("Request timeout", shouldRetry: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing task {TaskId}", task.Id);
            return SyncResult.Failure(ex.Message, shouldRetry: true);
        }
    }

    public TimeSpan CalculateRetryDelay(int attemptCount)
    {
        if (!_options.Handlers.TryGetValue(ServiceName, out var config))
        {
            // Default exponential backoff: 1min, 2min, 4min
            return TimeSpan.FromMinutes(Math.Pow(2, attemptCount));
        }

        var baseDelay = TimeSpan.FromSeconds(config.BaseDelaySeconds);
        var multiplier = Math.Pow(config.BackoffMultiplier, attemptCount);
        var delay = TimeSpan.FromTicks((long)(baseDelay.Ticks * multiplier));

        var maxDelay = TimeSpan.FromMinutes(config.MaxDelayMinutes);
        return delay > maxDelay ? maxDelay : delay;
    }

    private async Task ProcessTelegramAction(TelegramSyncPayload payload, string botToken, SyncTask task, CancellationToken cancellationToken)
    {
        switch (payload.Action.ToUpperInvariant())
        {
            case "CREATE":
                await ProcessCreateAction(payload, botToken, task);
                break;

            case "UPDATE":
                await ProcessUpdateAction(payload, botToken, task);
                break;

            case "DELETE":
                await ProcessDeleteAction(payload, botToken, task);
                break;

            default:
                throw new ArgumentException($"Unknown action: {payload.Action}");
        }
    }

    private async Task ProcessCreateAction(TelegramSyncPayload payload, string botToken, SyncTask task)
    {
        _logger.LogDebug("Creating Telegram message in channel {ChannelId}", payload.ChannelId);

        int messageId;
        if (payload.FullContent.Length > 4096) // Telegram's text message limit
        {
            // For long messages, send as file with Markdown fallback
            try
            {
                var longMessage = await _telegramService.SendLongMessageAsFileAsync(
                    botToken,
                    payload.ChannelId,
                    payload.FullContent,
                    payload.IsMarkdown ? ".md" : ".txt");
                messageId = longMessage.MessageId;

                _logger.LogDebug("Successfully created long message file {MessageId} in channel {ChannelId}",
                    messageId, payload.ChannelId);
            }
            catch (ApiRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to send long message with Markdown for note {NoteId} in channel {ChannelId}. Retrying without Markdown.",
                    task.EntityId, payload.ChannelId);

                if (payload.IsMarkdown)
                {
                    // Fallback: retry sending long message as plain text file
                    var longMessage = await _telegramService.SendLongMessageAsFileAsync(
                        botToken,
                        payload.ChannelId,
                        payload.FullContent,
                        ".txt");
                    messageId = longMessage.MessageId;

                    _logger.LogDebug("Successfully created long message file {MessageId} in channel {ChannelId} without Markdown",
                        messageId, payload.ChannelId);
                }
                else
                {
                    throw; // Re-throw if it wasn't a markdown issue
                }
            }
        }
        else
        {
            // For short messages, try Markdown first, fallback to plain text
            try
            {
                var message = await _telegramService.SendMessageAsync(
                    botToken,
                    payload.ChannelId,
                    payload.FullContent,
                    payload.IsMarkdown);
                messageId = message.MessageId;

                _logger.LogDebug("Successfully created message {MessageId} in channel {ChannelId}",
                    messageId, payload.ChannelId);
            }
            catch (ApiRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to send message with Markdown for note {NoteId} in channel {ChannelId}. Retrying without Markdown.",
                    task.EntityId, payload.ChannelId);

                if (payload.IsMarkdown)
                {
                    // Fallback: retry sending without markdown
                    var message = await _telegramService.SendMessageAsync(
                        botToken,
                        payload.ChannelId,
                        payload.FullContent,
                        false);
                    messageId = message.MessageId;

                    _logger.LogDebug("Successfully created message {MessageId} in channel {ChannelId} without Markdown",
                        messageId, payload.ChannelId);
                }
                else
                {
                    throw; // Re-throw if it wasn't a markdown issue
                }
            }
        }

        // Update note with the new message ID
        await AddMessageIdToNote(task.EntityId, payload.ChannelId, messageId);

        _logger.LogDebug("Successfully added message {MessageId} to note {NoteId} TelegramMessageIds",
            messageId, task.EntityId);
    }

    private async Task ProcessUpdateAction(TelegramSyncPayload payload, string botToken, SyncTask task)
    {
        if (!payload.MessageId.HasValue)
        {
            throw new ArgumentException("MessageId is required for UPDATE action");
        }

        _logger.LogDebug("Updating Telegram message {MessageId} in channel {ChannelId}",
            payload.MessageId.Value, payload.ChannelId);

        // CRITICAL: The SyncEditNote should only send UPDATE for messages <= 4096 chars
        // But add safety check in case of inconsistency
        if (payload.FullContent.Length > 4096)
        {
            _logger.LogWarning("UPDATE action received for long content ({Length} chars) - this should have been DELETE+CREATE. Processing as DELETE+CREATE.",
                payload.FullContent.Length);

            // Delete the old message and create a new one
            await _telegramService.DeleteMessageAsync(botToken, payload.ChannelId, payload.MessageId.Value);

            // Create new message and update note with new messageId
            await ProcessCreateAction(payload, botToken, task);
        }
        else
        {
            // For short messages, try to edit with Markdown first, fallback to plain text
            try
            {
                await _telegramService.EditMessageAsync(
                    botToken,
                    payload.ChannelId,
                    payload.MessageId.Value,
                    payload.FullContent,
                    payload.IsMarkdown);

                _logger.LogDebug("Successfully updated message {MessageId} in channel {ChannelId}",
                    payload.MessageId.Value, payload.ChannelId);
            }
            catch (ApiRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to edit message with Markdown for note {NoteId} in channel {ChannelId}. Retrying without Markdown.",
                    task.EntityId, payload.ChannelId);

                if (payload.IsMarkdown)
                {
                    // Fallback: retry editing without markdown
                    await _telegramService.EditMessageAsync(
                        botToken,
                        payload.ChannelId,
                        payload.MessageId.Value,
                        payload.FullContent,
                        false);

                    _logger.LogDebug("Successfully updated message {MessageId} in channel {ChannelId} without Markdown",
                        payload.MessageId.Value, payload.ChannelId);
                }
                else
                {
                    throw; // Re-throw if it wasn't a markdown issue
                }
            }
        }
    }

    private async Task ProcessDeleteAction(TelegramSyncPayload payload, string botToken, SyncTask task)
    {
        if (!payload.MessageId.HasValue)
        {
            throw new ArgumentException("MessageId is required for DELETE action");
        }

        _logger.LogDebug("Deleting Telegram message {MessageId} from channel {ChannelId}",
            payload.MessageId.Value, payload.ChannelId);

        // Delete from Telegram first
        await _telegramService.DeleteMessageAsync(botToken, payload.ChannelId, payload.MessageId.Value);

        // Update note to remove this specific message ID from TelegramMessageIds
        await RemoveMessageIdFromNote(task.EntityId, payload.ChannelId, payload.MessageId.Value);

        _logger.LogDebug("Successfully removed message {MessageId} from note {NoteId} TelegramMessageIds",
            payload.MessageId.Value, task.EntityId);
    }

    private async Task AddMessageIdToNote(long noteId, string channelId, int messageId)
    {
        // First get current TelegramMessageIds to compute the new value
        var currentNote = await _noteRepository.GetFirstOrDefaultAsync(
            n => n.Id == noteId,
            orderBy: null);

        if (currentNote == null)
        {
            _logger.LogWarning("Note {NoteId} not found, cannot add Telegram message ID", noteId);
            return;
        }

        var messageIdEntry = $"{channelId}:{messageId}";
        string updatedMessageIds;

        if (string.IsNullOrWhiteSpace(currentNote.TelegramMessageIds))
        {
            updatedMessageIds = messageIdEntry;
        }
        else
        {
            var messageIds = currentNote.TelegramMessageIds.Split(',').ToList();
            var existingIndex = messageIds.FindIndex(id => id.StartsWith($"{channelId}:"));

            if (existingIndex >= 0)
            {
                messageIds[existingIndex] = messageIdEntry;
            }
            else
            {
                messageIds.Add(messageIdEntry);
            }

            updatedMessageIds = string.Join(",", messageIds);
        }

        // Use precise update - only update TelegramMessageIds field
        await _noteRepository.UpdateAsync(
            note => new Note { TelegramMessageIds = updatedMessageIds },
            note => note.Id == noteId);

        _logger.LogDebug("Updated note {NoteId} TelegramMessageIds after adding {ChannelId}:{MessageId}",
            noteId, channelId, messageId);
    }

    private async Task RemoveMessageIdFromNote(long noteId, string channelId, int messageId)
    {
        // Get current TelegramMessageIds to compute the new value
        var currentNote = await _noteRepository.GetFirstOrDefaultAsync(
            n => n.Id == noteId,
            orderBy: null);

        if (currentNote == null || string.IsNullOrWhiteSpace(currentNote.TelegramMessageIds))
        {
            _logger.LogWarning("Note {NoteId} not found or has no Telegram message IDs", noteId);
            return;
        }

        var messageIds = currentNote.TelegramMessageIds.Split(',').ToList();
        var targetId = $"{channelId}:{messageId}";

        messageIds.RemoveAll(id => id.Equals(targetId, StringComparison.OrdinalIgnoreCase));

        string? updatedMessageIds = messageIds.Any() ? string.Join(",", messageIds) : null;

        // Use precise update - only update TelegramMessageIds field
        await _noteRepository.UpdateAsync(
            note => new Note { TelegramMessageIds = updatedMessageIds },
            note => note.Id == noteId);

        _logger.LogDebug("Updated note {NoteId} TelegramMessageIds after removing {ChannelId}:{MessageId}",
            noteId, channelId, messageId);
    }

    private static bool IsRetryableException(ApiRequestException ex)
    {
        // Common retryable error codes
        return ex.ErrorCode switch
        {
            429 => true, // Too Many Requests
            502 => true, // Bad Gateway
            503 => true, // Service Unavailable
            504 => true, // Gateway Timeout
            _ => ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                 ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase)
        };
    }
}
