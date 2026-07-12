namespace TransactionApproval.Application.DTOs;

/// <summary>
/// Request to simulate a transaction. The client sends the region and the
/// absolute instant to evaluate (ISO-8601 with offset, e.g. produced by
/// <c>Date.toISOString()</c>). The approval decision is made entirely on the
/// server; the client's own time zone math is never trusted.
/// </summary>
public record SimulateTransactionRequest(string RegionCode, DateTimeOffset SubmittedAt);
