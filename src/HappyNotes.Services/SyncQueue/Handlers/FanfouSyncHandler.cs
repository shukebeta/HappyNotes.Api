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

namespace HappyNotes.Services.SyncQueue.Handlers;

public class FanfouSyncHandler : ISyncHandler
{
    private readonly IFanfouStatusService _fanfouStatusService;
    private readonly IFanfouUserAccountCacheService _fanfouUserAccountCacheService;
    private readonly ITextToImageService _textToImageService;
    private readonly INoteRepository _noteRepository;
    private readonly SyncQueueOptions _options;
    private readonly JwtConfig _jwtConfig;
    private readonly ILogger<FanfouSyncHandler> _logger;

    public string ServiceName => Constants.FanfouService;

    public int MaxRetryAttempts => _options.Handlers.TryGetValue(ServiceName, out var config)
        ? config.MaxRetries
        : 5;

    public FanfouSyncHandler(
        IFanfouStatusService fanfouStatusService,
        IFanfouUserAccountCacheService fanfouUserAccountCacheService,
        ITextToImageService textToImageService,
        INoteRepository noteRepository,
        IOptions<SyncQueueOptions> options,
        IOptions<JwtConfig> jwtConfig,
        ILogger<FanfouSyncHandler> logger)
    {
        _fanfouStatusService = fanfouStatusService;
        _fanfouUserAccountCacheService = fanfouUserAccountCacheService;
        _textToImageService = textToImageService;
        _noteRepository = noteRepository;
        _options = options.Value;
        _jwtConfig = jwtConfig.Value;
        _logger = logger;
    }

    public async Task<SyncResult> ProcessAsync(SyncTask task, CancellationToken cancellationToken)
    {
        try
        {
            FanfouSyncPayload? payload;

            // Handle both typed and untyped task objects
            if (task.Payload is JsonElement jsonElement)
            {
                payload = JsonSerializer.Deserialize<FanfouSyncPayload>(jsonElement.GetRawText(), JsonSerializerConfig.Default);
            }
            else if (task.Payload is FanfouSyncPayload fanfouPayload)
            {
                payload = fanfouPayload;
            }
            else
            {
                return SyncResult.Failure("Invalid payload type", shouldRetry: false);
            }

            if (payload == null)
            {
                return SyncResult.Failure("Failed to deserialize payload", shouldRetry: false);
            }

            // Get FanfouUserAccount from cache
            var userAccounts = await _fanfouUserAccountCacheService.GetAsync(task.UserId);
            var userAccount = userAccounts.FirstOrDefault(a => a.Id == payload.UserAccountId);

            if (userAccount == null)
            {
                return SyncResult.Failure($"No Fanfou user account found for user {task.UserId} and account {payload.UserAccountId}", shouldRetry: false);
            }

            var consumerKey = TextEncryptionHelper.Decrypt(userAccount.ConsumerKey, _jwtConfig.SymmetricSecurityKey);
            var consumerSecret = TextEncryptionHelper.Decrypt(userAccount.ConsumerSecret, _jwtConfig.SymmetricSecurityKey);
            var accessToken = userAccount.DecryptedAccessToken(_jwtConfig.SymmetricSecurityKey);
            var accessTokenSecret = userAccount.DecryptedAccessTokenSecret(_jwtConfig.SymmetricSecurityKey);

            return task.Action.ToUpper() switch
            {
                "CREATE" => await ProcessCreateAction(task, payload, consumerKey, consumerSecret, accessToken, accessTokenSecret, cancellationToken),
                "DELETE" => await ProcessDeleteAction(task, payload, consumerKey, consumerSecret, accessToken, accessTokenSecret, cancellationToken),
                _ => SyncResult.Failure($"Unknown action: {task.Action}", shouldRetry: false)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Fanfou sync task {TaskId} for user {UserId}: {Error}",
                task.Id, task.UserId, ex.Message);
            return SyncResult.Failure(ex.Message);
        }
    }

    private async Task<SyncResult> ProcessCreateAction(
        SyncTask task,
        FanfouSyncPayload payload,
        string consumerKey,
        string consumerSecret,
        string accessToken,
        string accessTokenSecret,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Processing CREATE action for task {TaskId}", task.Id);

            // Strip markdown for content length check (but preserve for image generation)
            var plainContent = payload.FullContent;

            // Check if content exceeds 140 characters
            if (plainContent.Length > Constants.FanfouStatusLength)
            {
                _logger.LogDebug("Content exceeds {CharLimit} characters, generating image for task {TaskId}",
                    Constants.FanfouStatusLength, task.Id);

                // Generate image using shared service (same as Mastodon)
                var imageData = await _textToImageService.GenerateImageAsync(
                    payload.FullContent,
                    payload.IsMarkdown,
                    width: 600,
                    cancellationToken);

                // Upload via Fanfou's /photos/upload
                var statusId = await _fanfouStatusService.SendStatusWithPhotoAsync(
                    consumerKey, consumerSecret, accessToken, accessTokenSecret,
                    imageData,
                    statusText: null,
                    cancellationToken);

                // Add the status ID to the note
                await AddStatusIdToNote(task.EntityId, payload.UserAccountId, statusId);

                _logger.LogDebug("Successfully created photo status {StatusId} for task {TaskId}",
                    statusId, task.Id);

                return SyncResult.Success();
            }
            else
            {
                // Send as normal text status
                var statusId = await _fanfouStatusService.SendStatusAsync(
                    consumerKey, consumerSecret, accessToken, accessTokenSecret,
                    plainContent,
                    cancellationToken);

                // Add the status ID to the note
                await AddStatusIdToNote(task.EntityId, payload.UserAccountId, statusId);

                _logger.LogDebug("Successfully created text status {StatusId} for task {TaskId}",
                    statusId, task.Id);

                return SyncResult.Success();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create status for task {TaskId}: {Error}",
                task.Id, ex.Message);
            return SyncResult.Failure(ex.Message);
        }
    }

    private async Task<SyncResult> ProcessDeleteAction(
        SyncTask task,
        FanfouSyncPayload payload,
        string consumerKey,
        string consumerSecret,
        string accessToken,
        string accessTokenSecret,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(payload.StatusId))
            {
                return SyncResult.Failure("StatusId is required for DELETE action", shouldRetry: false);
            }

            _logger.LogDebug("Processing DELETE action for task {TaskId}, status {StatusId}",
                task.Id, payload.StatusId);

            await _fanfouStatusService.DeleteStatusAsync(
                consumerKey, consumerSecret, accessToken, accessTokenSecret,
                payload.StatusId,
                cancellationToken);

            // Remove the status ID from the note
            await RemoveStatusIdFromNote(task.EntityId, payload.UserAccountId, payload.StatusId);

            _logger.LogDebug("Successfully deleted status {StatusId} for task {TaskId}",
                payload.StatusId, task.Id);

            return SyncResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete status {StatusId} for task {TaskId}: {Error}",
                payload.StatusId, task.Id, ex.Message);
            return SyncResult.Failure(ex.Message);
        }
    }

    public TimeSpan CalculateRetryDelay(int attemptCount)
    {
        if (_options.Handlers.TryGetValue(ServiceName, out var config))
        {
            var delay = TimeSpan.FromSeconds(config.BaseDelaySeconds * Math.Pow(config.BackoffMultiplier, attemptCount - 1));
            var maxDelay = TimeSpan.FromMinutes(config.MaxDelayMinutes);
            return delay > maxDelay ? maxDelay : delay;
        }

        // Default fallback
        return TimeSpan.FromMinutes(Math.Min(Math.Pow(2, attemptCount - 1), 30));
    }

    private async Task AddStatusIdToNote(long noteId, long userAccountId, string statusId)
    {
        // First get current FanfouStatusIds to compute the new value
        var currentNote = await _noteRepository.GetFirstOrDefaultAsync(
            n => n.Id == noteId,
            orderBy: null);

        if (currentNote == null)
        {
            _logger.LogWarning("Note {NoteId} not found, cannot add Fanfou status ID", noteId);
            return;
        }

        currentNote.AddFanfouStatusId(userAccountId, statusId);

        // Use precise update - only update FanfouStatusIds field
        await _noteRepository.UpdateAsync(
            note => new Note { FanfouStatusIds = currentNote.FanfouStatusIds },
            note => note.Id == noteId);

        _logger.LogDebug("Updated note {NoteId} FanfouStatusIds after adding {UserAccountId}:{StatusId}",
            noteId, userAccountId, statusId);
    }

    private async Task RemoveStatusIdFromNote(long noteId, long userAccountId, string statusId)
    {
        // Get current FanfouStatusIds to compute the new value
        var currentNote = await _noteRepository.GetFirstOrDefaultAsync(
            n => n.Id == noteId,
            orderBy: null);

        if (currentNote == null || string.IsNullOrWhiteSpace(currentNote.FanfouStatusIds))
        {
            _logger.LogWarning("Note {NoteId} not found or has no Fanfou status IDs", noteId);
            return;
        }

        currentNote.RemoveFanfouStatusId(userAccountId, statusId);

        // Use precise update - only update FanfouStatusIds field
        await _noteRepository.UpdateAsync(
            note => new Note { FanfouStatusIds = currentNote.FanfouStatusIds },
            note => note.Id == noteId);

        _logger.LogDebug("Updated note {NoteId} FanfouStatusIds after removing {UserAccountId}:{StatusId}",
            noteId, userAccountId, statusId);
    }
}
