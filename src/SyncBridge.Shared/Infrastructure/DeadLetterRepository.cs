using Dapper;
using SyncBridge.Shared.Infrastructure.Models;

namespace SyncBridge.Shared.Infrastructure;

public sealed class DeadLetterRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public DeadLetterRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(
        string jobName,
        string? sourceId,
        string phase,
        string payload,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO sync_dead_letter
            (
                job_name,
                source_id,
                phase,
                payload,
                error_message,
                created_at
            )
            VALUES
            (
                @JobName,
                @SourceId,
                @Phase,
                CAST(@Payload AS jsonb),
                @ErrorMessage,
                NOW()
            );
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    JobName = jobName,
                    SourceId = sourceId,
                    Phase = phase,
                    Payload = payload,
                    ErrorMessage = errorMessage
                },
                cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<DeadLetterRecord>> GetPendingAsync(string jobName, int limit, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id AS Id,
                job_name AS JobName,
                source_id AS SourceId,
                phase AS Phase,
                payload::text AS Payload,
                error_message AS ErrorMessage,
                created_at AS CreatedAt,
                status AS Status,
                retry_count AS RetryCount,
                last_retried_at AS LastRetriedAt,
                resolved_at AS ResolvedAt,
                replay_error_message AS ReplayErrorMessage
            FROM sync_dead_letter
            WHERE job_name = @JobName
              AND status IN ('Pending', 'Failed')
            ORDER BY created_at ASC
            LIMIT @Limit;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QueryAsync<DeadLetterRecord>(
            new CommandDefinition(sql, new { JobName = jobName, Limit = limit }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<DeadLetterRecord>> GetAsync(
        string? jobName,
        string? status,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var sql = """
            SELECT
                id AS Id,
                job_name AS JobName,
                source_id AS SourceId,
                phase AS Phase,
                payload::text AS Payload,
                error_message AS ErrorMessage,
                created_at AS CreatedAt,
                status AS Status,
                retry_count AS RetryCount,
                last_retried_at AS LastRetriedAt,
                resolved_at AS ResolvedAt,
                replay_error_message AS ReplayErrorMessage
            FROM sync_dead_letter
            WHERE 1=1
            """;

        if (!string.IsNullOrEmpty(jobName)) sql += " AND job_name = @JobName";
        if (!string.IsNullOrEmpty(status)) sql += " AND status = @Status";

        sql += """
            
            ORDER BY created_at ASC
            LIMIT @Limit;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var records = await connection.QueryAsync<DeadLetterRecord>(
            new CommandDefinition(sql, new { JobName = jobName, Status = status, Limit = limit }, cancellationToken: cancellationToken));
            
        return records.ToList().AsReadOnly();
    }

    public async Task<DeadLetterRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id AS Id,
                job_name AS JobName,
                source_id AS SourceId,
                phase AS Phase,
                payload::text AS Payload,
                error_message AS ErrorMessage,
                created_at AS CreatedAt,
                status AS Status,
                retry_count AS RetryCount,
                last_retried_at AS LastRetriedAt,
                resolved_at AS ResolvedAt,
                replay_error_message AS ReplayErrorMessage
            FROM sync_dead_letter
            WHERE id = @Id;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<DeadLetterRecord>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<bool> MarkRetryingAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE sync_dead_letter
            SET status = 'Retrying',
                retry_count = retry_count + 1,
                last_retried_at = NOW()
            WHERE id = @Id
              AND status IN ('Pending', 'Failed');
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return affected > 0;
    }

    public async Task MarkResolvedAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE sync_dead_letter
            SET status = 'Resolved',
                resolved_at = NOW(),
                replay_error_message = NULL
            WHERE id = @Id;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task MarkReplayFailedAsync(long id, string errorMessage, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE sync_dead_letter
            SET status = 'Failed',
                replay_error_message = @ErrorMessage
            WHERE id = @Id;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, ErrorMessage = errorMessage }, cancellationToken: cancellationToken));
    }

    public async Task MarkIgnoredAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE sync_dead_letter
            SET status = 'Ignored'
            WHERE id = @Id;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
