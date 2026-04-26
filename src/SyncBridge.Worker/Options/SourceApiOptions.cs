namespace SyncBridge.Worker.Options;

public sealed class SourceApiOptions
{
    public const string SectionName = "SourceApi";

    public string BaseUrl { get; init; } = string.Empty;
    public int PageSize { get; init; } = 5;
    public int TimeoutSeconds { get; init; } = 30;
}
