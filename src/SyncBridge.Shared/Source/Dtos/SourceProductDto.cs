namespace SyncBridge.Shared.Source.Dtos;

public sealed class SourceProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}
