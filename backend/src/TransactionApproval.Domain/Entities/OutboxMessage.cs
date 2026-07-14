namespace TransactionApproval.Domain.Entities;

/// <summary>
/// A durable record of a domain event awaiting publication, written in the
/// same transaction as the state change that produced it (transactional
/// outbox pattern).
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }

    /// <summary>Event type discriminator, e.g. "TransactionApproved".</summary>
    public required string Type { get; set; }

    /// <summary>JSON-serialized event payload.</summary>
    public required string Payload { get; set; }

    /// <summary>When the event occurred, in UTC.</summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>When this message was successfully published, in UTC.</summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// When this message was moved to dead-letter after exhausting retries.
    /// </summary>
    public DateTime? DeadLetteredAtUtc { get; set; }

    /// <summary>
    /// Number of dispatch attempts performed so far.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Earliest UTC timestamp when this message is eligible for another
    /// processing attempt.
    /// </summary>
    public DateTime AvailableAtUtc { get; set; }

    /// <summary>
    /// Lease expiration UTC timestamp used to avoid multiple workers handling
    /// the same message concurrently.
    /// </summary>
    public DateTime? LeasedUntilUtc { get; set; }

    /// <summary>Last error message recorded from a failed publish attempt.</summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Optimistic concurrency token used to prevent multiple workers claiming
    /// the same message.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
