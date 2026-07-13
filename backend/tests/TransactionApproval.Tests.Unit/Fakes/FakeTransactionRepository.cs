using TransactionApproval.Application.Abstractions;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Tests.Unit.Fakes;

public sealed class FakeTransactionRepository : ITransactionRepository
{
    public List<Transaction> Transactions { get; } = [];
    public List<OutboxMessage> OutboxMessages { get; } = [];
    private readonly IReadOnlyList<Transaction> _approvedItems;
    private readonly int _totalCount;

    public FakeTransactionRepository(IReadOnlyList<Transaction>? approvedItems = null, int? totalCount = null)
    {
        _approvedItems = approvedItems ?? [];
        _totalCount = totalCount ?? _approvedItems.Count;
    }

    public Task AddAsync(
        Transaction transaction,
        IReadOnlyList<OutboxMessage> outboxMessages,
        CancellationToken cancellationToken)
    {
        Transactions.Add(transaction);
        OutboxMessages.AddRange(outboxMessages);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetApprovedAsync(
        int page, int pageSize, CancellationToken cancellationToken)
        => Task.FromResult((_approvedItems, _totalCount));
}
