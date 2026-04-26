using Microsoft.Extensions.Options;
using SyncBridge.Worker.Options;

namespace SyncBridge.Worker.Infrastructure;

public sealed class RetryExecutor
{
    private readonly SyncOptions _options;
    private readonly ILogger<RetryExecutor> _logger;

    public RetryExecutor(IOptions<SyncOptions> options, ILogger<RetryExecutor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(
        string operationName,
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await action(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (attempt <= _options.MaxRetries)
            {
                _logger.LogWarning(
                    ex,
                    "Operation {OperationName} failed on attempt {Attempt}/{MaxRetries}. Retrying in {DelayMilliseconds}ms.",
                    operationName,
                    attempt,
                    _options.MaxRetries,
                    _options.RetryDelayMilliseconds);

                await Task.Delay(_options.RetryDelayMilliseconds, cancellationToken);
            }
        }
    }
}
