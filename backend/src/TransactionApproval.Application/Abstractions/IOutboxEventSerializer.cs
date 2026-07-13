using TransactionApproval.Application.Events;

namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Deserializes outbox payloads to strongly typed transaction events.
/// </summary>
public interface IOutboxEventSerializer
{
    (string EventType, string Payload) Serialize(TransactionEvent @event);

    TransactionEvent Deserialize(string eventType, string payload);
}
