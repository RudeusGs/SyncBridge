using System.Text.Json;
using Microsoft.Extensions.Options;
using SyncBridge.Shared.Destination;
using SyncBridge.Worker.Infrastructure;
using SyncBridge.Shared.Infrastructure;
using SyncBridge.Shared.Mapping;
using SyncBridge.Worker.Options;
using SyncBridge.Worker.Source;
using SyncBridge.Shared.Source.Dtos;

namespace SyncBridge.Worker.Core;

public sealed class SyncEngine
{
    private readonly ISourceAdapter _sourceAdapter;
    private readonly IDestinationAdapter _destinationAdapter;
    private readonly CheckpointRepository _checkpointRepository;
    private readonly SyncLogRepository _syncLogRepository;
    private readonly DeadLetterRepository _deadLetterRepository;
    private readonly RetryExecutor _retryExecutor;
    private readonly SourceApiOptions _sourceApiOptions;
    private readonly SyncOptions _syncOptions;
    private readonly ILogger<SyncEngine> _logger;

    public SyncEngine(
        ISourceAdapter sourceAdapter,
        IDestinationAdapter destinationAdapter,
        CheckpointRepository checkpointRepository,
        SyncLogRepository syncLogRepository,
        DeadLetterRepository deadLetterRepository,
        RetryExecutor retryExecutor,
        IOptions<SourceApiOptions> sourceApiOptions,
        IOptions<SyncOptions> syncOptions,
        ILogger<SyncEngine> logger)
    {
        _sourceAdapter = sourceAdapter;
        _destinationAdapter = destinationAdapter;
        _checkpointRepository = checkpointRepository;
        _syncLogRepository = syncLogRepository;
        _deadLetterRepository = deadLetterRepository;
        _retryExecutor = retryExecutor;
        _sourceApiOptions = sourceApiOptions.Value;
        _syncOptions = syncOptions.Value;
        _logger = logger;
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        var jobName = _syncOptions.JobName;
        var logId = await _syncLogRepository.StartAsync(jobName, cancellationToken);

        var successfulCount = 0;
        DateTime? cursorUpdatedAt = null;
        int? cursorId = null;

        try
        {
            var checkpoint = await _checkpointRepository.GetAsync(jobName, cancellationToken);

            cursorUpdatedAt = checkpoint?.LastSyncedAt?.ToUniversalTime();
            cursorId = checkpoint?.LastSyncedId;

            _logger.LogInformation(
                "Starting sync cycle. Job={JobName}, CursorUpdatedAt={CursorUpdatedAt}, CursorId={CursorId}",
                jobName,
                cursorUpdatedAt,
                cursorId);

            while (!cancellationToken.IsCancellationRequested)
            {
                var page = await _retryExecutor.ExecuteAsync(
                    "fetch_source_page",
                    ct => _sourceAdapter.GetProductsPageAsync(
                        cursorUpdatedAt,
                        cursorId,
                        _sourceApiOptions.PageSize,
                        ct),
                    cancellationToken);

                if (page.Items.Count == 0)
                {
                    _logger.LogInformation("No new source records found.");
                    break;
                }

                var syncedAt = DateTime.UtcNow;
                var mappedProducts = page.Items
                    .Select(item => ProductMapper.Map(item, syncedAt))
                    .ToList();

                try
                {
                    await _destinationAdapter.UpsertProductsAsync(mappedProducts, cancellationToken);
                    successfulCount += mappedProducts.Count;
                }
                catch (Exception batchException)
                {
                    _logger.LogWarning(
                        batchException,
                        "Batch upsert failed. Falling back to record-by-record mode.");

                    for (var i = 0; i < page.Items.Count; i++)
                    {
                        var sourceItem = page.Items[i];
                        var mappedItem = mappedProducts[i];

                        try
                        {
                            await _destinationAdapter.UpsertProductAsync(mappedItem, cancellationToken);
                            successfulCount++;
                        }
                        catch (Exception itemException)
                        {
                            var payload = JsonSerializer.Serialize(sourceItem);

                            await _deadLetterRepository.InsertAsync(
                                jobName,
                                sourceItem.Id.ToString(),
                                "upsert_product",
                                payload,
                                itemException.ToString(),
                                cancellationToken);

                            _logger.LogError(
                                itemException,
                                "Failed to upsert source product {SourceId}. Sent to dead letter.",
                                sourceItem.Id);
                        }
                    }
                }

                if (page.NextCursor is not null)
                {
                    cursorUpdatedAt = page.NextCursor.UpdatedAt.ToUniversalTime();
                    cursorId = page.NextCursor.Id;
                }

                if (!page.HasMore)
                {
                    break;
                }
            }

            await _checkpointRepository.SaveAsync(
                jobName,
                cursorUpdatedAt,
                cursorId,
                cancellationToken);

            await _syncLogRepository.CompleteAsync(
                logId,
                "Succeeded",
                successfulCount,
                null,
                cancellationToken);

            _logger.LogInformation(
                "Sync cycle completed successfully. Job={JobName}, SuccessfulCount={SuccessfulCount}, CursorUpdatedAt={CursorUpdatedAt}, CursorId={CursorId}",
                jobName,
                successfulCount,
                cursorUpdatedAt,
                cursorId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _syncLogRepository.CompleteAsync(
                logId,
                "Failed",
                successfulCount,
                ex.Message,
                cancellationToken);

            _logger.LogError(ex, "Sync cycle failed.");
        }
    }
}
