using HappyNotes.Services.SyncQueue.Configuration;
using HappyNotes.Services.SyncQueue.Models;
using HappyNotes.Services.SyncQueue.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using StackExchange.Redis;

namespace HappyNotes.Services.Tests.SyncQueue;

[TestFixture]
[Category("Integration")]
public class DelayedTaskProcessingTests
{
    private RedisSyncQueueService _queueService = null!;
    private IConnectionMultiplexer _redis = null!;
    private IDatabase _database = null!;
    private string _redisConnectionString = null!;

    [SetUp]
    public void Setup()
    {
        // Get Redis connection string from environment variable
        _redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                               ?? Environment.GetEnvironmentVariable("TEST_REDIS_CONNECTION_STRING")
                               ?? "localhost:6379";

        try
        {
            _redis = ConnectionMultiplexer.Connect(_redisConnectionString);
            _database = _redis.GetDatabase(15); // Use database 15 for tests

            var options = Options.Create(new SyncQueueOptions
            {
                Redis = new RedisOptions
                {
                    ConnectionString = _redisConnectionString,
                    Database = 15,
                    KeyPrefix = "test:delayed:"
                },
                Processing = new ProcessingOptions
                {
                    VisibilityTimeout = TimeSpan.FromMinutes(15)
                }
            });

            var logger = new LoggerFactory().CreateLogger<RedisSyncQueueService>();
            _queueService = new RedisSyncQueueService(_redis, options, logger, TimeProvider.System);
        }
        catch (Exception ex)
        {
            Assert.Ignore($"Redis is not available for testing: {ex.Message}. Set REDIS_CONNECTION_STRING environment variable to run integration tests.");
        }
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (_redis != null && _redis.IsConnected)
            {
                // Clean up test data using dynamic server endpoint
                var endpoint = _redis.GetEndPoints().FirstOrDefault();
                if (endpoint != null)
                {
                    var server = _redis.GetServer(endpoint);
                    var keys = server.Keys(database: 15, pattern: "test:delayed:*");
                    if (keys.Any())
                    {
                        _database.KeyDelete(keys.ToArray());
                    }
                }
            }

            _redis?.Dispose();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Test]
    public async Task ProcessDelayedTasks_ShouldHandleLargeBatchAtomically()
    {
        // Arrange - Create many delayed tasks that are ready
        var pastTime = DateTime.UtcNow.AddMinutes(-5);

        for (int i = 0; i < 150; i++) // More than batch limit of 100
        {
            var task = SyncTask.Create("telegram", "CREATE", 100 + i, 456, new { content = $"batch test {i}" });
            task.ScheduledFor = pastTime;
            await _queueService.ScheduleRetryAsync("telegram", task, TimeSpan.Zero);
        }

        var delayedKey = "test:delayed:telegram:delayed";
        var queueKey = "test:delayed:telegram:queue";

        // Verify all tasks are in delayed queue
        var delayedCount = await _database.SortedSetLengthAsync(delayedKey);
        Assert.That(delayedCount, Is.EqualTo(150), "All tasks should be in delayed queue");

        // Act - Process delayed tasks (should handle batch limit)
        var initialStats = await _queueService.GetStatsAsync("telegram");

        // Trigger delayed processing by attempting dequeue (which calls ProcessDelayedTasksWithRateLimit)
        var dequeuedTask = await _queueService.DequeueAsync<object>("telegram", CancellationToken.None);

        // Assert - Should process up to batch limit atomically
        var afterStats = await _queueService.GetStatsAsync("telegram");
        var remainingDelayed = await _database.SortedSetLengthAsync(delayedKey);
        var queueLength = await _database.ListLengthAsync(queueKey);

        // Should have moved exactly batch limit (100) or remaining tasks if less
        Assert.That(remainingDelayed, Is.LessThanOrEqualTo(50), "Should have processed at least 100 tasks");
        Assert.That(queueLength, Is.GreaterThan(0), "Should have moved tasks to main queue");
        Assert.That(dequeuedTask, Is.Not.Null, "Should have dequeued one task from newly moved batch");

        // Verify atomicity - total tasks should remain constant
        var totalTasks = remainingDelayed + queueLength + (dequeuedTask != null ? 1 : 0);
        Assert.That(totalTasks, Is.EqualTo(150), "Total task count should remain consistent");
    }

    [Test]
    public async Task ProcessDelayedTasks_EmptyDelayed_ShouldHandleGracefully()
    {
        // Act - Process delayed tasks when none exist
        var task = await _queueService.DequeueAsync<object>("telegram", CancellationToken.None);

        // Assert - Should return null without errors
        Assert.That(task, Is.Null, "Should handle empty delayed queue gracefully");
    }

    [Test]
    public async Task ProcessDelayedTasks_FutureTasks_ShouldLeaveUntouched()
    {
        // Arrange - Create task scheduled for future
        var futureTask = SyncTask.Create("telegram", "CREATE", 123, 456, new { content = "future test" });
        var futureTime = DateTime.UtcNow.AddMinutes(5);
        futureTask.ScheduledFor = futureTime;

        await _queueService.ScheduleRetryAsync("telegram", futureTask, TimeSpan.FromMinutes(5));

        // Act - Try to process delayed tasks
        var dequeuedTask = await _queueService.DequeueAsync<object>("telegram", CancellationToken.None);

        // Assert - Future task should remain in delayed queue
        var delayedKey = "test:delayed:telegram:delayed";
        var delayedCount = await _database.SortedSetLengthAsync(delayedKey);

        Assert.That(dequeuedTask, Is.Null, "Should not dequeue future task");
        Assert.That(delayedCount, Is.EqualTo(1), "Future task should remain in delayed queue");
    }
}
