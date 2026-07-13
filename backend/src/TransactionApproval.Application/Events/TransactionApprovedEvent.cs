namespace TransactionApproval.Application.Events;

/// <summary>
/// Event payload published when a transaction simulation is approved.
/// Carries the minimum required fields: event type, alert id, outcome, and
/// a timestamp, plus light region context for traceability.
/// </summary>
public record TransactionApprovedEvent(
    Guid AlertId,
    DateTime TimestampUtc,
    string RegionCode,
    string RegionName)
    : TransactionEvent(AlertId, TimestampUtc, RegionCode, RegionName)
{
    public override string EventType => EventTypes.TransactionApproved;
}
