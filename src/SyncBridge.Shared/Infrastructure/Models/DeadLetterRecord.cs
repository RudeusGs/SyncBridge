using System.Text.Json;

namespace SyncBridge.Shared.Infrastructure.Models;

public class DeadLetterRecord
{
    public long Id { get; init; }
    public string JobName { get; init; } = string.Empty;
    public string? SourceId { get; init; }
    public string Phase { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public int RetryCount { get; init; }
    public DateTimeOffset? LastRetriedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
    public string? ReplayErrorMessage { get; init; }
}
