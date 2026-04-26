using Dapper;
using SyncBridge.Shared.Destination.Models;
using SyncBridge.Shared.Infrastructure;

namespace SyncBridge.Shared.Destination;

public sealed class PostgresDestinationAdapter : IDestinationAdapter
{
    private readonly DbConnectionFactory _connectionFactory;

    private const string UpsertSql = """
        INSERT INTO products
        (
            id,
            name,
            sku,
            price,
            currency,
            source_updated_at,
            synced_at
        )
        VALUES
        (
            @Id,
            @Name,
            @Sku,
            @Price,
            @Currency,
            @SourceUpdatedAt,
            @SyncedAt
        )
        ON CONFLICT (id) DO UPDATE
        SET
            name = EXCLUDED.name,
            sku = EXCLUDED.sku,
            price = EXCLUDED.price,
            currency = EXCLUDED.currency,
            source_updated_at = EXCLUDED.source_updated_at,
            synced_at = EXCLUDED.synced_at;
        """;

    public PostgresDestinationAdapter(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task UpsertProductsAsync(
        IReadOnlyList<ProductEntity> products,
        CancellationToken cancellationToken = default)
    {
        if (products.Count == 0)
        {
            return;
        }

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                UpsertSql,
                products,
                transaction,
                cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task UpsertProductAsync(
        ProductEntity product,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                UpsertSql,
                product,
                cancellationToken: cancellationToken));
    }
}
