using SyncBridge.Worker.Source.Dtos;
using SyncBridge.Shared.Source.Dtos;

namespace SyncBridge.Worker.Source;

public interface ISourceAdapter
{
    Task<ProductPageDto<SourceProductDto>> GetProductsPageAsync(
        DateTime? updatedAfter,
        int? afterId,
        int pageSize,
        CancellationToken cancellationToken = default);
}
