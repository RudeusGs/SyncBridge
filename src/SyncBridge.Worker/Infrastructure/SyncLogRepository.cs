using Dapper;
using SyncBridge.Shared.Infrastructure;

namespace SyncBridge.Worker.Infrastructure;

public sealed class SyncLogRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public SyncLogRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> StartAsync(
        string jobName,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO sync_log
            (
                job_name,
                started_at,
                status,
                processed_count
            )
            VALUES
            (
                @JobName,
                NOW(),
                'Running',
                0
            )
            RETURNING id;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                sql,
                new { JobName = jobName },
                cancellationToken: cancellationToken));
    }

    public async Task CompleteAsync(
        long logId,
        string status,
        int processedCount,
        string? errorMessage,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE sync_log
            SET
                ended_at = NOW(),
                status = @Status,
                processed_count = @ProcessedCount,
                error_message = @ErrorMessage
            WHERE id = @LogId;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    LogId = logId,
                    Status = status,
                    ProcessedCount = processedCount,
                    ErrorMessage = errorMessage
                },
                cancellationToken: cancellationToken));
    }
}
