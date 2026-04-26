namespace SyncBridge.SourceMockApi.Models;

public sealed class ProductPageResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public bool HasMore { get; init; }
    public SyncCursorResponse? NextCursor { get; init; }
}