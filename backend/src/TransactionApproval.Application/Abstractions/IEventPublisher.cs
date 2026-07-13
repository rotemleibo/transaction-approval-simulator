using TransactionApproval.Application.Events;

namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Publishes a strongly typed transaction event to an external channel. The
/// default implementation logs the event; a future implementation could
/// publish to a message broker without changing calling code.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(TransactionEvent @event, CancellationToken cancellationToken);
}
