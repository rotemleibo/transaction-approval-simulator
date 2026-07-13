using Microsoft.EntityFrameworkCore;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Infrastructure.Persistence.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly AppDbContext _db;

    public OutboxRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(
        int batchSize,
        DateTime nowUtc,
        DateTime leasedUntilUtc,
        CancellationToken cancellationToken)
    {
        var messages = await _db.OutboxMessages
            .Where(m =>
                m.ProcessedOnUtc == null &&
                m.DeadLetteredAtUtc == null &&
                m.AvailableAtUtc <= nowUtc &&
                (m.LeasedUntilUtc == null || m.LeasedUntilUtc <= nowUtc))
            .OrderBy(m => m.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return messages;
        }

        foreach (var message in messages)
        {
            message.LeasedUntilUtc = leasedUntilUtc;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return messages;
    }

    public async Task MarkProcessedAsync(Guid id, DateTime processedOnUtc, CancellationToken cancellationToken)
    {
        var message = await _db.OutboxMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message is null)
        {
            return;
        }

        message.ProcessedOnUtc = processedOnUtc;
        message.LeasedUntilUtc = null;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid id,
        string error,
        DateTime nextAttemptAtUtc,
        CancellationToken cancellationToken)
    {
        var message = await _db.OutboxMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message is null)
        {
            return;
        }

        message.Attempts += 1;
        message.LastError = error;
        message.AvailableAtUtc = nextAttemptAtUtc;
        message.LeasedUntilUtc = null;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkDeadLetteredAsync(
        Guid id,
        DateTime deadLetteredAtUtc,
        string reason,
        CancellationToken cancellationToken)
    {
        var message = await _db.OutboxMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message is null)
        {
            return;
        }

        message.Attempts += 1;
        message.DeadLetteredAtUtc = deadLetteredAtUtc;
        message.LastError = reason;
        message.LeasedUntilUtc = null;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
