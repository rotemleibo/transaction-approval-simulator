using Microsoft.Extensions.Logging;
using System.Text.Json;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Application.Events;

namespace TransactionApproval.Infrastructure.Messaging;

/// <summary>
/// Publishes events by writing a structured log line. This is the default
/// publisher for the simulator; a message-broker-backed implementation could
/// replace it later without touching calling code.
/// </summary>
public class LogEventPublisher : IEventPublisher
{
    private readonly ILogger<LogEventPublisher> _logger;
    private readonly IOutboxEventSerializer _serializer;

    public LogEventPublisher(ILogger<LogEventPublisher> logger, IOutboxEventSerializer serializer)
    {
        _logger = logger;
        _serializer = serializer;
    }

    public Task PublishAsync(TransactionEvent @event, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (eventType, payload) = _serializer.Serialize(@event);
        _logger.LogInformation("Published alert event {EventType}: {Payload}", eventType, payload);

        return Task.CompletedTask;
    }
}