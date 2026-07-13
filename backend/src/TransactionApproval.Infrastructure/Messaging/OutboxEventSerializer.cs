using System.Text.Json;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Application.Events;

namespace TransactionApproval.Infrastructure.Messaging;

public sealed class OutboxEventSerializer : IOutboxEventSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyDictionary<string, Type> EventTypeMap = new Dictionary<string, Type>(StringComparer.Ordinal)
    {
        [EventTypes.TransactionApproved] = typeof(TransactionApprovedEvent),
        [EventTypes.TransactionRejected] = typeof(TransactionRejectedEvent)
    };

    public (string EventType, string Payload) Serialize(TransactionEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (!EventTypeMap.TryGetValue(@event.EventType, out _))
        {
            throw new NotSupportedException($"Unknown outbox event type '{@event.EventType}'.");
        }

        var payload = JsonSerializer.Serialize(@event, @event.GetType(), SerializerOptions);
        return (@event.EventType, payload);
    }

    public TransactionEvent Deserialize(string eventType, string payload)
    {
        if (!EventTypeMap.TryGetValue(eventType, out var clrType))
        {
            throw new NotSupportedException($"Unknown outbox event type '{eventType}'.");
        }

        if (JsonSerializer.Deserialize(payload, clrType, SerializerOptions) is not TransactionEvent @event)
        {
            throw new InvalidOperationException($"Failed to deserialize outbox payload for event type '{eventType}'.");
        }

        return @event;
    }
}
