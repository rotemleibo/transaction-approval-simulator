using Microsoft.EntityFrameworkCore;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Domain.Entities;
using TransactionApproval.Domain.Enums;

namespace TransactionApproval.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;

    public TransactionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(
        Transaction transaction,
        IReadOnlyList<OutboxMessage> outboxMessages,
        CancellationToken cancellationToken)
    {
        await _db.Transactions.AddAsync(transaction, cancellationToken);

        if (outboxMessages.Count > 0)
        {
            await _db.OutboxMessages.AddRangeAsync(outboxMessages, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetApprovedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _db.Transactions
            .AsNoTracking()
            .Where(t => t.Status == TransactionStatus.Approved)
            .OrderByDescending(t => t.CreatedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
