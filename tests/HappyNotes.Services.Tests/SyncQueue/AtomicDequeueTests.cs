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
public class AtomicDequeueTests
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
                    KeyPrefix = "test:atomic:"
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
                    var keys = server.Keys(database: 15, pattern: "test:atomic:*");
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
    public async Task AtomicDequeue_ShouldMoveTaskFromQueueToProcessingAtomically()
    {
        // Arrange
        var task = SyncTask.Create("telegram", "CREATE", 123, 456, new { content = "atomic test" });
        await _queueService.EnqueueAsync("telegram", task);

        // Verify task is in queue, not in processing
        var queueKey = "test:atomic:telegram:queue";
        var processingKey = "test:atomic:telegram:processing";

        var queueLength = await _database.ListLengthAsync(queueKey);
        var processingLength = await _database.SortedSetLengthAsync(processingKey);

        Assert.That(queueLength, Is.EqualTo(1), "Task should be in queue");
        Assert.That(processingLength, Is.EqualTo(0), "Processing should be empty");

        // Act - Atomic dequeue
        var dequeuedTask = await _queueService.DequeueAsync<object>("telegram", CancellationToken.None);

        // Assert - Task moved atomically
        Assert.That(dequeuedTask, Is.Not.Null, "Task should be dequeued");
        Assert.That(dequeuedTask.Id, Is.EqualTo(task.Id), "Dequeued task ID should match");

        // Verify atomic operation: task removed from queue AND added to processing
        queueLength = await _database.ListLengthAsync(queueKey);
        processingLength = await _database.SortedSetLengthAsync(processingKey);

        Assert.That(queueLength, Is.EqualTo(0), "Queue should be empty after dequeue");
        Assert.That(processingLength, Is.EqualTo(1), "Processing should have one task");

        // Verify the processing entry has correct expiry
        var processingEntries = await _database.SortedSetRangeByScoreWithScoresAsync(processingKey);
        Assert.That(processingEntries.Length, Is.EqualTo(1), "Should have one processing entry");

        var expiryTime = DateTimeOffset.FromUnixTimeSeconds((long)processingEntries[0].Score);
        var expectedExpiry = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(15));
        var timeDiff = Math.Abs((expiryTime - expectedExpiry).TotalMinutes);

        Assert.That(timeDiff, Is.LessThan(1), "Expiry time should be approximately 15 minutes from now");
    }

    [Test]
    public async Task AtomicDequeue_EmptyQueue_ShouldReturnNull()
    {
        // Act
        var result = await _queueService.DequeueAsync<object>("telegram", CancellationToken.None);

        // Assert
        Assert.That(result, Is.Null, "Dequeue from empty queue should return null");

        // Verify no side effects on processing queue
        var processingKey = "test:atomic:telegram:processing";
        var processingLength = await _database.SortedSetLengthAsync(processingKey);
        Assert.That(processingLength, Is.EqualTo(0), "Processing queue should remain empty");
    }
}
