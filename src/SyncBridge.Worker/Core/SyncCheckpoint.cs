namespace SyncBridge.Worker.Core;

public sealed class SyncCheckpoint
{
    public string JobName { get; init; } = string.Empty;
    public DateTime? LastSyncedAt { get; init; }
    public int? LastSyncedId { get; init; }
    public DateTime UpdatedAt { get; init; }
}
