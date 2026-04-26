using System.Text.Json;
using SyncBridge.Worker.Source.Dtos;
using SyncBridge.Shared.Source.Dtos;
using Microsoft.Extensions.Logging;

namespace SyncBridge.Worker.Source;

public sealed class ApiSourceAdapter : ISourceAdapter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiSourceAdapter> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiSourceAdapter(HttpClient httpClient, ILogger<ApiSourceAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductPageDto<SourceProductDto>> GetProductsPageAsync(
        DateTime? updatedAfter,
        int? afterId,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>
        {
            $"pageSize={pageSize}"
        };

        if (updatedAfter.HasValue)
        {
            queryParts.Add($"updatedAfter={Uri.EscapeDataString(updatedAfter.Value.ToUniversalTime().ToString("O"))}");
        }

        if (afterId.HasValue)
        {
            queryParts.Add($"afterId={afterId.Value}");
        }

        var url = "/api/products";
        if (queryParts.Count > 0)
        {
            url += "?" + string.Join("&", queryParts);
        }

        _logger.LogInformation("Fetching source page from {Url}", url);

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var page = await JsonSerializer.DeserializeAsync<ProductPageDto<SourceProductDto>>(
            stream,
            JsonOptions,
            cancellationToken);

        return page ?? new ProductPageDto<SourceProductDto>();
    }
}
