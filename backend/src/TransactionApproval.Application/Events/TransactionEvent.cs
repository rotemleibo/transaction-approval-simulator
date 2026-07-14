namespace TransactionApproval.Application.Events;

/// <summary>
/// Common fields shared by all transaction decision events: event type,
/// transaction id, timestamp, plus light region context for
/// traceability.
/// </summary>
public abstract record TransactionEvent(
    Guid TransactionId,
    DateTime TimestampUtc,
    string RegionCode,
    string RegionName)
{
    public abstract string EventType { get; }
}
