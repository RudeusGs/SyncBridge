using SyncBridge.Shared.Destination.Models;

namespace SyncBridge.Shared.Destination;

public interface IDestinationAdapter
{
    Task UpsertProductsAsync(
        IReadOnlyList<ProductEntity> products,
        CancellationToken cancellationToken = default);

    Task UpsertProductAsync(
        ProductEntity product,
        CancellationToken cancellationToken = default);
}
