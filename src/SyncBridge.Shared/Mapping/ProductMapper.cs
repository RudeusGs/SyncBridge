using SyncBridge.Shared.Destination.Models;
using SyncBridge.Shared.Source.Dtos;

namespace SyncBridge.Shared.Mapping;

public static class ProductMapper
{
    public static ProductEntity Map(SourceProductDto source, DateTime syncedAtUtc)
    {
        return new ProductEntity
        {
            Id = source.Id,
            Name = source.Name,
            Sku = source.Sku,
            Price = source.Price,
            Currency = source.Currency,
            SourceUpdatedAt = source.UpdatedAt.ToUniversalTime(),
            SyncedAt = syncedAtUtc.ToUniversalTime()
        };
    }
}
