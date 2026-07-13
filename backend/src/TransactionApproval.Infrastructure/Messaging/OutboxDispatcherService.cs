using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Infrastructure.Messaging;

/// <summary>
/// Polls the outbox table for pending messages and dispatches each to the
/// configured <see cref="IEventPublisher"/>, marking success/failure with a
/// bounded retry count.
/// </summary>
public class OutboxDispatcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxDispatcherOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OutboxDispatcherService> _logger;

    public OutboxDispatcherService(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxDispatcherOptions> options,
        TimeProvider timeProvider,
        ILogger<OutboxDispatcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.PollingIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Outbox dispatch cycle failed unexpectedly.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Shutting down.
            }
        }
    }

    private async Task DispatchPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var serializer = scope.ServiceProvider.GetRequiredService<IOutboxEventSerializer>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var leasedUntil = now + _options.LeaseDuration;
        var claimed = await outbox.ClaimPendingAsync(_options.BatchSize, now, leasedUntil, cancellationToken);

        foreach (var message in claimed)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await TryDispatchAsync(message, outbox, serializer, publisher, cancellationToken);
        }
    }

    private async Task TryDispatchAsync(
        OutboxMessage message,
        IOutboxRepository outbox,
        IOutboxEventSerializer serializer,
        IEventPublisher publisher,
        CancellationToken cancellationToken)
    {
        try
        {
            var @event = serializer.Deserialize(message.Type, message.Payload);
            await publisher.PublishAsync(@event, cancellationToken);
            await outbox.MarkProcessedAsync(message.Id, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

            _logger.LogInformation(
                "OutboxMessagePublished MessageId={MessageId} Type={MessageType}",
                message.Id,
                message.Type);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var attemptNumber = message.Attempts + 1;

            if (attemptNumber >= _options.MaxRetryCount)
            {
                _logger.LogError(
                    ex,
                    "OutboxMessageDeadLettered MessageId={MessageId} Type={MessageType} Attempts={Attempts}",
                    message.Id,
                    message.Type,
                    attemptNumber);

                await outbox.MarkDeadLetteredAsync(
                    message.Id,
                    _timeProvider.GetUtcNow().UtcDateTime,
                    ex.Message,
                    cancellationToken);

                return;
            }

            var nextAttemptAtUtc = _timeProvider.GetUtcNow().UtcDateTime + ComputeBackoff(attemptNumber);

            _logger.LogWarning(
                ex,
                "OutboxMessagePublishFailed MessageId={MessageId} Type={MessageType} Attempt={Attempt} NextAttemptAt={NextAttemptAt}",
                message.Id,
                message.Type,
                attemptNumber,
                nextAttemptAtUtc);

            await outbox.MarkFailedAsync(message.Id, ex.Message, nextAttemptAtUtc, cancellationToken);
        }
    }

    private TimeSpan ComputeBackoff(int attemptNumber)
    {
        var exponent = Math.Min(attemptNumber - 1, 30);
        var backoffTicks = _options.BaseBackoff.Ticks * Math.Pow(2, exponent);

        if (double.IsInfinity(backoffTicks) || backoffTicks >= _options.MaxBackoff.Ticks)
        {
            return _options.MaxBackoff;
        }

        return TimeSpan.FromTicks((long)backoffTicks);
    }
}
