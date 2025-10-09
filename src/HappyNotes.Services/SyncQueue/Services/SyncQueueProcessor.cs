using HappyNotes.Services.SyncQueue.Configuration;
using HappyNotes.Services.SyncQueue.Interfaces;
using HappyNotes.Services.SyncQueue.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyNotes.Services.SyncQueue.Services;

public class SyncQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISyncQueueService _queueService;
    private readonly SyncQueueOptions _options;
    private readonly ILogger<SyncQueueProcessor> _logger;

    public SyncQueueProcessor(
        IServiceProvider serviceProvider,
        ISyncQueueService queueService,
        IOptions<SyncQueueOptions> options,
        ILogger<SyncQueueProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _queueService = queueService;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SyncQueueProcessor started");

        // Get registered service names from a temporary scope
        var serviceNames = GetRegisteredServiceNames();

        if (!serviceNames.Any())
        {
            _logger.LogWarning("No sync handlers registered, processor will not process any tasks");
            return;
        }

        var processingTasks = serviceNames.Select(serviceName =>
            ProcessServiceQueue(serviceName, stoppingToken)).ToArray();

        // Start recovery task for all services
        var recoveryTask = RunRecoveryLoop(serviceNames, stoppingToken);

        try
        {
            await Task.WhenAll(processingTasks.Concat(new[] { recoveryTask }).ToArray());
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SyncQueueProcessor was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in SyncQueueProcessor - service will continue running");
            // Don't throw - let the service continue running to maintain resilience
        }
        finally
        {
            _logger.LogInformation("SyncQueueProcessor stopped");
        }
    }

    private string[] GetRegisteredServiceNames()
    {
        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<ISyncHandler>();
        return handlers.Select(h => h.ServiceName).ToArray();
    }

    private async Task ProcessServiceQueue(string serviceName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started processing queue for service: {ServiceName}", serviceName);

        using var semaphore = new SemaphoreSlim(_options.Processing.MaxConcurrentTasks);
        var exitReason = "normal";

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await semaphore.WaitAsync(cancellationToken);

                    var task = await _queueService.DequeueAsync<object>(serviceName, cancellationToken);

                    if (task == null)
                    {
                        semaphore.Release();
                        await Task.Delay(_options.Processing.PollingInterval, cancellationToken);
                        continue;
                    }

                    // Process task in background with fresh scope per task
                    _ = Task.Run(async () =>
                    {
                        using var taskScope = _serviceProvider.CreateScope();
                        try
                        {
                            // Resolve handler fresh for each task (with clean scoped dependencies)
                            var handler = taskScope.ServiceProvider.GetServices<ISyncHandler>()
                                .FirstOrDefault(h => h.ServiceName == serviceName);

                            if (handler == null)
                            {
                                _logger.LogError("No handler found for service {ServiceName}", serviceName);
                                return;
                            }

                            var syncTask = new SyncTask
                            {
                                Id = task.Id,
                                Service = task.Service,
                                Action = task.Action,
                                EntityId = task.EntityId,
                                UserId = task.UserId,
                                Payload = task.Payload,
                                AttemptCount = task.AttemptCount,
                                CreatedAt = task.CreatedAt,
                                ScheduledFor = task.ScheduledFor,
                                Metadata = task.Metadata
                            };
                            await ProcessTask(handler, syncTask, cancellationToken);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    exitReason = "cancelled";
                    break;
                }
                catch (Exception ex)
                {
                    semaphore.Release(); // Release semaphore to prevent deadlock
                    _logger.LogError(ex, "Error in queue processing loop for service {ServiceName}", serviceName);
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
            }
        }
        finally
        {
            if (exitReason == "cancelled")
            {
                _logger.LogWarning("Processing queue for service {ServiceName} was cancelled at {StopTime}",
                    serviceName, DateTime.UtcNow);
            }
            else
            {
                _logger.LogInformation("Processing queue for service {ServiceName} stopped normally at {StopTime}",
                    serviceName, DateTime.UtcNow);
            }
        }
    }

    private async Task ProcessTask(ISyncHandler handler, SyncTask task, CancellationToken cancellationToken)
    {
        var serviceName = handler.ServiceName;
        var taskId = task.Id;

        _logger.LogDebug("Processing task {TaskId} for service {ServiceName}", taskId, serviceName);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.Processing.ProcessingTimeout);

            var result = await handler.ProcessAsync(task, timeoutCts.Token);

            if (result.IsSuccess)
            {
                await _queueService.RemoveFromProcessingAsyncOnSuccess(serviceName, task);
                _logger.LogDebug("Successfully processed task {TaskId}", taskId);
            }
            else
            {
                await HandleFailedTask(handler, task, result.ErrorMessage ?? "Unknown error");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Task {TaskId} processing was cancelled", taskId);
            // Task will remain in processing queue and be retried later
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing task {TaskId}", taskId);
            await HandleFailedTask(handler, task, ex.Message);
        }
    }

    private async Task HandleFailedTask(ISyncHandler handler, SyncTask task, string errorMessage)
    {
        var serviceName = handler.ServiceName;
        var taskId = task.Id;

        if (task.AttemptCount >= handler.MaxRetryAttempts)
        {
            _logger.LogWarning("Task {TaskId} exceeded max retry attempts ({MaxAttempts}), moving to failed queue",
                taskId, handler.MaxRetryAttempts);

            await _queueService.MoveToFailedAsync(serviceName, task, errorMessage);
        }
        else
        {
            var retryDelay = handler.CalculateRetryDelay(task.AttemptCount);

            _logger.LogWarning("Task {TaskId} failed (attempt {AttemptCount}/{MaxAttempts}), scheduling retry in {RetryDelay}: {Error}",
                taskId, task.AttemptCount + 1, handler.MaxRetryAttempts, retryDelay, errorMessage);

            await _queueService.RemoveFromProcessingAsync(serviceName, task);
            await _queueService.ScheduleRetryAsync(serviceName, task, retryDelay);
        }
    }

    private async Task RunRecoveryLoop(IEnumerable<string> serviceNames, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started processing queue recovery task");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                foreach (var serviceName in serviceNames)
                {
                    await _queueService.RecoverExpiredTasksAsync(serviceName);
                }

                await Task.Delay(_options.Processing.RecoveryInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in recovery loop - will retry in 5 minutes");
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }

        _logger.LogInformation("Stopped processing queue recovery task");
    }
}
