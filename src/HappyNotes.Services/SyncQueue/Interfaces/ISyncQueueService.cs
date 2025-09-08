using HappyNotes.Services.SyncQueue.Models;

namespace HappyNotes.Services.SyncQueue.Interfaces;

public interface ISyncQueueService
{
    /// <summary>
    /// Enqueue a task for processing
    /// </summary>
    Task EnqueueAsync<T>(string service, SyncTask<T> task);
    
    /// <summary>
    /// Dequeue a task from the main queue
    /// </summary>
    Task<SyncTask<T>?> DequeueAsync<T>(string service, CancellationToken cancellationToken);
    
    /// <summary>
    /// Schedule a task for retry with delay
    /// </summary>
    Task ScheduleRetryAsync<T>(string service, SyncTask<T> task, TimeSpan delay);
    
    /// <summary>
    /// Move a task to the failed queue
    /// </summary>
    Task MoveToFailedAsync<T>(string service, SyncTask<T> task, string error);
    
    /// <summary>
    /// Get queue statistics for a service
    /// </summary>
    Task<QueueStats> GetStatsAsync(string service);
    
    /// <summary>
    /// Retry all failed tasks for a service
    /// </summary>
    Task RetryFailedTasksAsync(string service);
    
    /// <summary>
    /// Clear all tasks from a service queue
    /// </summary>
    Task ClearQueueAsync(string service);
    
    /// <summary>
    /// Mark a task as processing
    /// </summary>
    Task MarkAsProcessingAsync<T>(string service, SyncTask<T> task);
    
    /// <summary>
    /// Remove a task from processing queue
    /// </summary>
    Task RemoveFromProcessingAsync<T>(string service, SyncTask<T> task);
    
    /// <summary>
    /// Remove a task from processing queue and update success statistics
    /// </summary>
    Task RemoveFromProcessingAsyncOnSuccess<T>(string service, SyncTask<T> task);
    
    /// <summary>
    /// Recover expired tasks from processing queue
    /// </summary>
    Task RecoverExpiredTasksAsync(string service);
}