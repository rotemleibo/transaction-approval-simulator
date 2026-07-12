using TransactionApproval.Application.Abstractions;

namespace TransactionApproval.Infrastructure.Time;

/// <summary>Default clock backed by the operating system, always in UTC.</summary>
public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
