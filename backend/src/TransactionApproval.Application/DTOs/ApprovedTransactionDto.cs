namespace TransactionApproval.Application.DTOs;

/// <summary>An approved transaction as shown in the results list.</summary>
public record ApprovedTransactionDto(
    Guid Id,
    string RegionCode,
    string RegionName,
    string TimeZoneId,
    DateTime SubmittedUtc,
    DateTime LocalTransactionTime,
    DateTime CreatedAtUtc);
