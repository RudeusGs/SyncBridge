using Microsoft.AspNetCore.Mvc;
using SyncBridge.Shared.Core;
using SyncBridge.Shared.Destination;
using SyncBridge.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<DeadLetterRepository>();
builder.Services.AddSingleton<IDestinationAdapter, PostgresDestinationAdapter>();
builder.Services.AddSingleton<DeadLetterReplayService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.MapGet("/api/dead-letters", async (
    [FromQuery] string? jobName,
    [FromQuery] string? status,
    [FromQuery] int? limit,
    DeadLetterRepository repository,
    CancellationToken ct) =>
{
    var take = Math.Clamp(limit.GetValueOrDefault(50), 1, 200);
    var records = await repository.GetAsync(jobName, status, take, ct);
    return Results.Ok(records);
});

app.MapPost("/api/dead-letters/{id}/replay", async (
    long id,
    DeadLetterReplayService replayService,
    CancellationToken ct) =>
{
    try
    {
        await replayService.ReplayOneAsync(id, ct);
        return Results.Ok(new { Message = $"Replay triggered for {id}." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.MapPost("/api/dead-letters/replay", async (
    [FromBody] ReplayRequest request,
    DeadLetterReplayService replayService,
    CancellationToken ct) =>
{
    var attemptedCount = await replayService.ReplayPendingAsync(request.JobName, request.Limit, ct);
    return Results.Ok(new 
    { 
        Attempted = attemptedCount,
        Message = $"Attempted replay for {attemptedCount} records." 
    });
});

app.Run();

public class ReplayRequest
{
    public string JobName { get; set; } = "product-sync";
    public int Limit { get; set; } = 20;
}
