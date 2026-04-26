namespace SyncBridge.Worker.Source.Dtos;

public sealed class ProductPageDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public bool HasMore { get; init; }
    public SyncCursorDto? NextCursor { get; init; }
}
