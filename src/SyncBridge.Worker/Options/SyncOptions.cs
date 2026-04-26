namespace SyncBridge.Worker.Options;

public sealed class SyncOptions
{
    public const string SectionName = "Sync";

    public string JobName { get; init; } = "product-sync";
    public int IntervalSeconds { get; init; } = 20;
    public int MaxRetries { get; init; } = 3;
    public int RetryDelayMilliseconds { get; init; } = 1000;
    public int DeadLetterReplayBatchSize { get; init; } = 20;
}
