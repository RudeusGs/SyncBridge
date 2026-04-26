using Microsoft.Extensions.Options;
using SyncBridge.Worker.Core;
using SyncBridge.Worker.Options;

namespace SyncBridge.Worker.Workers;

public sealed class ProductSyncWorker : BackgroundService
{
    private readonly SyncEngine _syncEngine;
    private readonly SyncOptions _options;
    private readonly ILogger<ProductSyncWorker> _logger;

    public ProductSyncWorker(
        SyncEngine syncEngine,
        IOptions<SyncOptions> options,
        ILogger<ProductSyncWorker> logger)
    {
        _syncEngine = syncEngine;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.IntervalSeconds);

        _logger.LogInformation(
            "ProductSyncWorker started. IntervalSeconds={IntervalSeconds}",
            _options.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await _syncEngine.RunOnceAsync(stoppingToken);
            await Task.Delay(interval, stoppingToken);
        }
    }
}
