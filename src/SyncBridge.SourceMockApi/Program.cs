using SyncBridge.SourceMockApi.Data;
using SyncBridge.SourceMockApi.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var products = SeedProducts.Get()
    .OrderBy(p => p.UpdatedAt)
    .ThenBy(p => p.Id)
    .ToList();

app.MapGet("/", () => Results.Ok(new
{
    service = "SyncBridge.SourceMockApi",
    status = "running"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    time = DateTime.UtcNow
}));

app.MapGet("/api/products", (DateTime? updatedAfter, int? afterId, int? pageSize) =>
{
    var take = pageSize is > 0 and <= 500 ? pageSize.Value : 5;

    IEnumerable<SourceProductResponse> query = products;

    if (updatedAfter.HasValue)
    {
        var cursorTime = updatedAfter.Value.ToUniversalTime();
        var cursorId = afterId.GetValueOrDefault(0);

        query = query.Where(p =>
            p.UpdatedAt > cursorTime ||
            (p.UpdatedAt == cursorTime && p.Id > cursorId));
    }

    query = query
        .OrderBy(p => p.UpdatedAt)
        .ThenBy(p => p.Id);

    var pagePlusOne = query.Take(take + 1).ToList();
    var hasMore = pagePlusOne.Count > take;
    var items = pagePlusOne.Take(take).ToList();

    var nextCursor = items.Count == 0
        ? null
        : new SyncCursorResponse
        {
            UpdatedAt = items[^1].UpdatedAt,
            Id = items[^1].Id
        };

    var response = new ProductPageResponse<SourceProductResponse>
    {
        Items = items,
        HasMore = hasMore,
        NextCursor = nextCursor
    };

    return Results.Ok(response);
});

app.Run();