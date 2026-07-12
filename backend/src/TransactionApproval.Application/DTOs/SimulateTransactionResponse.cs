using TransactionApproval.Domain.Enums;

namespace TransactionApproval.Application.DTOs;

/// <summary>
/// Explains the outcome of a simulation: what was submitted, which time zone
/// resolved the region, the computed local time, and the final status.
/// </summary>
public record SimulateTransactionResponse(
    Guid Id,
    string RegionCode,
    string RegionName,
    string TimeZoneId,
    DateTime SubmittedUtc,
    DateTime LocalTransactionTime,
    TransactionStatus Status,
    string Reason);
