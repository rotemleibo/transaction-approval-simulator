using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Persistence for outbox messages produced by the transactional outbox
/// pattern.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Claims a batch of pending messages for processing by setting a lease
    /// window. Only messages eligible at <paramref name="nowUtc"/> and not
    /// already leased are claimed.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(
        int batchSize,
        DateTime nowUtc,
        DateTime leasedUntilUtc,
        CancellationToken cancellationToken);

    Task MarkProcessedAsync(
        Guid id,
        byte[] expectedRowVersion,
        DateTime processedOnUtc,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        Guid id,
        byte[] expectedRowVersion,
        string error,
        DateTime nextAttemptAtUtc,
        CancellationToken cancellationToken);

    Task MarkDeadLetteredAsync(
        Guid id,
        byte[] expectedRowVersion,
        DateTime deadLetteredAtUtc,
        string reason,
        CancellationToken cancellationToken);
}
