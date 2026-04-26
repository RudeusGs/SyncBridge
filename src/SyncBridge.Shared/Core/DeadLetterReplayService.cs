using System.Text.Json;
using Microsoft.Extensions.Logging;
using SyncBridge.Shared.Destination;
using SyncBridge.Shared.Destination.Models;
using SyncBridge.Shared.Infrastructure;
using SyncBridge.Shared.Mapping;
using SyncBridge.Shared.Source.Dtos;

namespace SyncBridge.Shared.Core;

public class DeadLetterReplayService
{
    private readonly DeadLetterRepository _repository;
    private readonly IDestinationAdapter _destinationAdapter;
    private readonly ILogger<DeadLetterReplayService> _logger;

    public DeadLetterReplayService(
        DeadLetterRepository repository,
        IDestinationAdapter destinationAdapter,
        ILogger<DeadLetterReplayService> logger)
    {
        _repository = repository;
        _destinationAdapter = destinationAdapter;
        _logger = logger;
    }

    public async Task ReplayOneAsync(long deadLetterId, CancellationToken cancellationToken = default)
    {
        var record = await _repository.GetByIdAsync(deadLetterId, cancellationToken);
        if (record == null)
        {
            throw new InvalidOperationException($"Dead-letter record {deadLetterId} not found.");
        }

        if (record.Status is not "Pending" and not "Failed")
        {
            _logger.LogWarning("Dead-letter record {Id} is in status {Status} and cannot be replayed.", record.Id, record.Status);
            return;
        }

        _logger.LogInformation("Replaying dead-letter record {Id} (Phase: {Phase})", record.Id, record.Phase);

        var updated = await _repository.MarkRetryingAsync(record.Id, cancellationToken);
        if (!updated)
        {
            _logger.LogWarning("Failed to mark dead-letter record {Id} as Retrying. It might be already processed.", record.Id);
            return;
        }

        try
        {
            if (record.Phase == "upsert_product")
            {
                SourceProductDto sourceDto;
                try
                {
                    sourceDto = JsonSerializer.Deserialize<SourceProductDto>(record.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? throw new JsonException("Deserialization returned null.");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize payload for dead-letter record {Id}", record.Id);
                    await _repository.MarkReplayFailedAsync(record.Id, $"Deserialization failed: {ex.Message}", cancellationToken);
                    return;
                }

                var entity = ProductMapper.Map(sourceDto, DateTime.UtcNow);
                await _destinationAdapter.UpsertProductAsync(entity, cancellationToken);
                await _repository.MarkResolvedAsync(record.Id, cancellationToken);
                
                _logger.LogInformation("Successfully replayed dead-letter record {Id}", record.Id);
            }
            else
            {
                _logger.LogWarning("Unknown phase '{Phase}' for dead-letter record {Id}. Marking as failed.", record.Phase, record.Id);
                await _repository.MarkReplayFailedAsync(record.Id, $"Unknown phase: {record.Phase}", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to replay dead-letter record {Id}", record.Id);
            await _repository.MarkReplayFailedAsync(record.Id, ex.Message, cancellationToken);
        }
    }

    public async Task<int> ReplayPendingAsync(string jobName, int limit, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting batch replay for job {JobName} (Limit: {Limit})", jobName, limit);
        
        var records = await _repository.GetPendingAsync(jobName, limit, cancellationToken);
        var attemptedCount = 0;

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await ReplayOneAsync(record.Id, cancellationToken);
                attemptedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during batch replay for record {Id}", record.Id);
            }
        }

        _logger.LogInformation("Completed batch replay. Processed {Count} records.", attemptedCount);
        return attemptedCount;
    }
}
