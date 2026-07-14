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
        var claimed = new List<OutboxMessage>(batchSize);

        while (claimed.Count < batchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var remaining = batchSize - claimed.Count;

            var candidates = await _db.OutboxMessages
                .Where(m =>
                    m.ProcessedOnUtc == null &&
                    m.DeadLetteredAtUtc == null &&
                    m.AvailableAtUtc <= nowUtc &&
                    (m.LeasedUntilUtc == null || m.LeasedUntilUtc <= nowUtc))
                .OrderBy(m => m.OccurredOnUtc)
                .Take(remaining)
                .ToListAsync(cancellationToken);

            if (candidates.Count == 0)
            {
                break;
            }

            foreach (var message in candidates)
            {
                message.LeasedUntilUtc = leasedUntilUtc;
            }

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                claimed.AddRange(candidates);
                break;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Another instance claimed one or more candidates first.
                foreach (var entry in ex.Entries)
                {
                    entry.State = EntityState.Detached;
                }

                foreach (var candidate in candidates)
                {
                    if (_db.Entry(candidate).State != EntityState.Detached)
                    {
                        _db.Entry(candidate).State = EntityState.Detached;
                    }
                }
            }
        }

        return claimed;
    }

    public async Task MarkProcessedAsync(
        Guid id,
        byte[] expectedRowVersion,
        DateTime processedOnUtc,
        CancellationToken cancellationToken)
    {
        await _db.OutboxMessages
            .Where(m =>
                m.Id == id &&
                m.RowVersion == expectedRowVersion &&
                m.ProcessedOnUtc == null &&
                m.DeadLetteredAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.ProcessedOnUtc, _ => processedOnUtc)
                    .SetProperty(m => m.LeasedUntilUtc, _ => null),
                cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid id,
        byte[] expectedRowVersion,
        string error,
        DateTime nextAttemptAtUtc,
        CancellationToken cancellationToken)
    {
        await _db.OutboxMessages
            .Where(m =>
                m.Id == id &&
                m.RowVersion == expectedRowVersion &&
                m.ProcessedOnUtc == null &&
                m.DeadLetteredAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.Attempts, m => m.Attempts + 1)
                    .SetProperty(m => m.LastError, _ => error)
                    .SetProperty(m => m.AvailableAtUtc, _ => nextAttemptAtUtc)
                    .SetProperty(m => m.LeasedUntilUtc, _ => null),
                cancellationToken);
    }

    public async Task MarkDeadLetteredAsync(
        Guid id,
        byte[] expectedRowVersion,
        DateTime deadLetteredAtUtc,
        string reason,
        CancellationToken cancellationToken)
    {
        await _db.OutboxMessages
            .Where(m =>
                m.Id == id &&
                m.RowVersion == expectedRowVersion &&
                m.ProcessedOnUtc == null &&
                m.DeadLetteredAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.Attempts, m => m.Attempts + 1)
                    .SetProperty(m => m.DeadLetteredAtUtc, _ => deadLetteredAtUtc)
                    .SetProperty(m => m.LastError, _ => reason)
                    .SetProperty(m => m.LeasedUntilUtc, _ => null),
                cancellationToken);
    }
}
