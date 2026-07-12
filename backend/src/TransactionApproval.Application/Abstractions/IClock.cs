namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Abstraction over the system clock so time-dependent logic stays testable.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
