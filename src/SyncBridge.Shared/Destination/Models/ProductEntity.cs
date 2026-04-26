namespace SyncBridge.Shared.Destination.Models;

public sealed class ProductEntity
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime SourceUpdatedAt { get; init; }
    public DateTime SyncedAt { get; init; }
}
