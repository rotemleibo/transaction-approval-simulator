using TransactionApproval.Application.Abstractions;

namespace TransactionApproval.Tests.Unit.Fakes;

public sealed class FakeClock : IClock
{
    public DateTime UtcNow { get; } = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
}
