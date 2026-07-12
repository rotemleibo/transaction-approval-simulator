using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Persistence for transaction simulations.
/// </summary>
public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);

    /// <summary>
    /// Returns approved transactions, newest first, with total count for paging.
    /// </summary>
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetApprovedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
