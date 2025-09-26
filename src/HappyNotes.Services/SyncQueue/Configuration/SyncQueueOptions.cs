namespace HappyNotes.Services.SyncQueue.Configuration;

public class SyncQueueOptions
{
    public const string SectionName = "SyncQueue";
    
    public RedisOptions Redis { get; set; } = new();
    public ProcessingOptions Processing { get; set; } = new();
    public Dictionary<string, HandlerOptions> Handlers { get; set; } = new();
}

public class RedisOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public int Database { get; set; } = 1;
    public string KeyPrefix { get; set; } = "happynotes:sync:";
}

public class ProcessingOptions
{
    public int MaxConcurrentTasks { get; set; } = 1;
    public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan VisibilityTimeout { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RecoveryInterval { get; set; } = TimeSpan.FromMinutes(5);
}

public class HandlerOptions
{
    public int MaxRetries { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 60;
    public double BackoffMultiplier { get; set; } = 2.0;
    public int MaxDelayMinutes { get; set; } = 60;
}