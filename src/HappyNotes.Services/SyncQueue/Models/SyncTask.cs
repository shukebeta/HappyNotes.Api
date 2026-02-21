using System.Text.Json;

namespace HappyNotes.Services.SyncQueue.Models;

public class SyncTask<T>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Service { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public long UserId { get; set; }
    public T Payload { get; set; } = default!;
    public int AttemptCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledFor { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SyncTask : SyncTask<object>
{
    public static SyncTask<T> Create<T>(string service, string action, long entityId, long userId, T payload)
    {
        return new SyncTask<T>
        {
            Service = service,
            Action = action,
            EntityId = entityId,
            UserId = userId,
            Payload = payload,
            ScheduledFor = DateTime.UtcNow
        };
    }
}

public class SyncResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShouldRetry { get; set; } = true;
    public TimeSpan? CustomRetryDelay { get; set; }

    public static SyncResult Success() => new() { IsSuccess = true };

    public static SyncResult Failure(string errorMessage, bool shouldRetry = true, TimeSpan? customRetryDelay = null) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ShouldRetry = shouldRetry,
            CustomRetryDelay = customRetryDelay
        };
}

public class QueueStats
{
    public string Service { get; set; } = string.Empty;
    public long PendingCount { get; set; }
    public long DelayedCount { get; set; }
    public long FailedCount { get; set; }
    public long ProcessingCount { get; set; }
    public DateTime LastProcessedAt { get; set; }
    public DateTime LastFailedAt { get; set; }
    public long TotalProcessed { get; set; }
    public long TotalFailed { get; set; }
}
