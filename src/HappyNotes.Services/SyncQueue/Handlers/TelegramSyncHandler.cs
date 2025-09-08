using System.Text.Json;
using Api.Framework.Helper;
using Api.Framework.Models;
using HappyNotes.Common;
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
        IOptions<SyncQueueOptions> options,
        IOptions<JwtConfig> jwtConfig,
        ILogger<TelegramSyncHandler> logger)
    {
        _telegramService = telegramService;
        _telegramSettingsCache = telegramSettingsCache;
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
            
            await ProcessTelegramAction(payload, decryptedToken, cancellationToken);

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

    private async Task ProcessTelegramAction(TelegramSyncPayload payload, string botToken, CancellationToken cancellationToken)
    {
        switch (payload.Action.ToUpperInvariant())
        {
            case "CREATE":
                await ProcessCreateAction(payload, botToken);
                break;
            
            case "UPDATE":
                await ProcessUpdateAction(payload, botToken);
                break;
            
            case "DELETE":
                await ProcessDeleteAction(payload, botToken);
                break;
            
            default:
                throw new ArgumentException($"Unknown action: {payload.Action}");
        }
    }

    private async Task ProcessCreateAction(TelegramSyncPayload payload, string botToken)
    {
        _logger.LogDebug("Creating Telegram message in channel {ChannelId}", payload.ChannelId);
        
        if (payload.FullContent.Length > 4096) // Telegram's text message limit
        {
            await _telegramService.SendLongMessageAsFileAsync(
                botToken, 
                payload.ChannelId, 
                payload.FullContent,
                payload.IsMarkdown ? ".md" : ".txt");
        }
        else
        {
            await _telegramService.SendMessageAsync(
                botToken, 
                payload.ChannelId, 
                payload.FullContent, 
                payload.IsMarkdown);
        }
    }

    private async Task ProcessUpdateAction(TelegramSyncPayload payload, string botToken)
    {
        if (!payload.MessageId.HasValue)
        {
            throw new ArgumentException("MessageId is required for UPDATE action");
        }

        _logger.LogDebug("Updating Telegram message {MessageId} in channel {ChannelId}", 
            payload.MessageId.Value, payload.ChannelId);

        // For updates, if the message is too long, we need to delete and recreate
        if (payload.FullContent.Length > 4096)
        {
            // Delete the old message and create a new one
            await _telegramService.DeleteMessageAsync(botToken, payload.ChannelId, payload.MessageId.Value);
            await ProcessCreateAction(payload, botToken);
        }
        else
        {
            await _telegramService.EditMessageAsync(
                botToken, 
                payload.ChannelId, 
                payload.MessageId.Value,
                payload.FullContent, 
                payload.IsMarkdown);
        }
    }

    private async Task ProcessDeleteAction(TelegramSyncPayload payload, string botToken)
    {
        if (!payload.MessageId.HasValue)
        {
            throw new ArgumentException("MessageId is required for DELETE action");
        }

        _logger.LogDebug("Deleting Telegram message {MessageId} from channel {ChannelId}", 
            payload.MessageId.Value, payload.ChannelId);

        await _telegramService.DeleteMessageAsync(botToken, payload.ChannelId, payload.MessageId.Value);
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