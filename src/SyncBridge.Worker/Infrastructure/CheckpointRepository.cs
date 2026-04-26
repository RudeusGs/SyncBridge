using Dapper;
using SyncBridge.Shared.Infrastructure;
using SyncBridge.Worker.Core;

namespace SyncBridge.Worker.Infrastructure;

public sealed class CheckpointRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CheckpointRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<SyncCheckpoint?> GetAsync(
        string jobName,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                job_name       AS JobName,
                last_synced_at AS LastSyncedAt,
                last_synced_id AS LastSyncedId,
                updated_at     AS UpdatedAt
            FROM sync_checkpoint
            WHERE job_name = @JobName;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<SyncCheckpoint>(
            new CommandDefinition(sql, new { JobName = jobName }, cancellationToken: cancellationToken));
    }

    public async Task SaveAsync(
        string jobName,
        DateTime? lastSyncedAt,
        int? lastSyncedId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO sync_checkpoint
            (
                job_name,
                last_synced_at,
                last_synced_id,
                updated_at
            )
            VALUES
            (
                @JobName,
                @LastSyncedAt,
                @LastSyncedId,
                NOW()
            )
            ON CONFLICT (job_name) DO UPDATE
            SET
                last_synced_at = EXCLUDED.last_synced_at,
                last_synced_id = EXCLUDED.last_synced_id,
                updated_at = EXCLUDED.updated_at;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    JobName = jobName,
                    LastSyncedAt = lastSyncedAt?.ToUniversalTime(),
                    LastSyncedId = lastSyncedId
                },
                cancellationToken: cancellationToken));
    }
}
