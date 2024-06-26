using System.Collections.Concurrent;
using HappyNotes.Models;
using Api.Framework.Helper;

namespace HappyNotes.Services;

public static class DuplicateRequestChecker
{
    private const int TimeThresholdInMilliseconds = 1500;
    private static readonly int ExpirationDurationInSeconds = 2;
    private const int CleanupIntervalInSeconds = 2;

    public static readonly ConcurrentDictionary<long, (DateTimeOffset Timestamp, string Hash)> RecentRequests = new();
    private static TimeSpan _expirationDuration = TimeSpan.FromSeconds(ExpirationDurationInSeconds);
    private static TimeSpan _cleanupInterval = TimeSpan.FromSeconds(CleanupIntervalInSeconds);
    private static readonly object CleanupLock = new();

    private static readonly CancellationTokenSource CancellationTokenSource = new();
    public static int Length => RecentRequests.Count;

    static DuplicateRequestChecker()
    {
        // Start the background cleanup task
        Task.Run(async () => await PeriodicCleanup(CancellationTokenSource.Token), CancellationTokenSource.Token);
    }

    public static bool IsDuplicate(long userId, PostNoteRequest request)
    {
        var now = DateTimeOffset.UtcNow;

        if (RecentRequests.TryGetValue(userId, out var lastRequest))
        {
            var contentHash = CommonHelper.CalculateMd5Hash(System.Text.Json.JsonSerializer.Serialize(request));
            if ((now - lastRequest.Timestamp).TotalMilliseconds < TimeThresholdInMilliseconds && lastRequest.Hash == contentHash)
            {
                return true;
            }
        }

        var newContentHash = CommonHelper.CalculateMd5Hash(System.Text.Json.JsonSerializer.Serialize(request));
        RecentRequests[userId] = (now, newContentHash);
        return false;
    }

    private static async Task PeriodicCleanup(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, cancellationToken);

                lock (CleanupLock)
                {
                    var now = DateTime.UtcNow;
                    var keysToRemove = RecentRequests
                        .Where(kv => (now - kv.Value.Timestamp).TotalMilliseconds > _expirationDuration.TotalMilliseconds)
                        .Select(kv => kv.Key)
                        .ToList();

                    foreach (var key in keysToRemove)
                    {
                        RecentRequests.TryRemove(key, out _);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, exit gracefully
                break;
            }
            catch (Exception ex)
            {
                // Log the exception using a logging framework
                Console.WriteLine($"Scheduled cleanup error: {ex.Message}");
            }
        }
    }
    // Method to set the cleanup interval dynamically
    public static void SetCleanupInterval(TimeSpan interval)
    {
        lock (CleanupLock)
        {
            _cleanupInterval = interval;
        }
    }

    public static void SetExpirationDuration(TimeSpan interval)
    {
        lock (CleanupLock)
        {
            _expirationDuration = interval;
        }
    }

    // Method to stop the background task gracefully
    public static void StopBackgroundTask()
    {
        CancellationTokenSource.Cancel();
    }
}
