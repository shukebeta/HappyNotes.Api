using HappyNotes.Services.SyncQueue.Models;

namespace HappyNotes.Services.SyncQueue.Interfaces;

public interface ISyncHandler
{
    /// <summary>
    /// The service name this handler processes (e.g., "telegram", "mastodon")
    /// </summary>
    string ServiceName { get; }
    
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    int MaxRetryAttempts { get; }
    
    /// <summary>
    /// Process a sync task
    /// </summary>
    Task<SyncResult> ProcessAsync(SyncTask task, CancellationToken cancellationToken);
    
    /// <summary>
    /// Calculate retry delay based on attempt count
    /// </summary>
    TimeSpan CalculateRetryDelay(int attemptCount);
}