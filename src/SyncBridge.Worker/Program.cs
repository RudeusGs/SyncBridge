using Microsoft.Extensions.Options;
using SyncBridge.Worker.Core;
using SyncBridge.Worker.Infrastructure;
using SyncBridge.Shared.Destination;
using SyncBridge.Shared.Infrastructure;
using SyncBridge.Shared.Core;
using SyncBridge.Worker.Options;
using SyncBridge.Worker.Source;
using SyncBridge.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<SourceApiOptions>(
    builder.Configuration.GetSection(SourceApiOptions.SectionName));

builder.Services.Configure<SyncOptions>(
    builder.Configuration.GetSection(SyncOptions.SectionName));

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<CheckpointRepository>();
builder.Services.AddSingleton<SyncLogRepository>();
builder.Services.AddSingleton<DeadLetterRepository>();
builder.Services.AddSingleton<RetryExecutor>();
builder.Services.AddSingleton<IDestinationAdapter, PostgresDestinationAdapter>();
builder.Services.AddSingleton<SyncEngine>();
builder.Services.AddSingleton<DeadLetterReplayService>();

builder.Services.AddHttpClient<ISourceAdapter, ApiSourceAdapter>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<SourceApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddHostedService<ProductSyncWorker>();

var host = builder.Build();
host.Run();
