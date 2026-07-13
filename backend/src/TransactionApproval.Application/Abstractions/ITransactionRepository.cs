using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Persistence for transaction simulations.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Persists the transaction along with any outbox messages produced by
    /// the decision, in a single atomic write.
    /// </summary>
    Task AddAsync(
        Transaction transaction,
        IReadOnlyList<OutboxMessage> outboxMessages,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns approved transactions, newest first, with total count for paging.
    /// </summary>
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetApprovedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
