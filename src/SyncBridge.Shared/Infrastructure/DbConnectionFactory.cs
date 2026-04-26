using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SyncBridge.Shared.Infrastructure;

public sealed class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TargetDb")
            ?? throw new InvalidOperationException("Connection string 'TargetDb' is missing.");
    }

    public async Task<NpgsqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
