namespace TransactionApproval.Application.Events;

/// <summary>
/// Event payload published when a transaction simulation is approved.
/// Carries the minimum required fields: event type, transaction id, and
/// a timestamp, plus light region context for traceability.
/// </summary>
public record TransactionApprovedEvent(
    Guid TransactionId,
    DateTime TimestampUtc,
    string RegionCode,
    string RegionName)
    : TransactionEvent(TransactionId, TimestampUtc, RegionCode, RegionName)
{
    public override string EventType => EventTypes.TransactionApproved;
}
