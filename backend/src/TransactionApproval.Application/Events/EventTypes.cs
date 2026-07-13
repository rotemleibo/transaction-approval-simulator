namespace TransactionApproval.Application.Events;

/// <summary>
/// Well-known event type discriminators used in outbox messages.
/// </summary>
public static class EventTypes
{
    public const string TransactionApproved = "TransactionApproved";
    public const string TransactionRejected = "TransactionRejected";
}
