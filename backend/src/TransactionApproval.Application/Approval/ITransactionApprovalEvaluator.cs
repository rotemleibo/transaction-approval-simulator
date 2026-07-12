using TransactionApproval.Domain.Enums;

namespace TransactionApproval.Application.Approval;

/// <summary>Result of evaluating the approval rule for a single instant.</summary>
public record ApprovalDecision(DateTime LocalTransactionTime, TransactionStatus Status, string Reason);

/// <summary>
/// Pure, dependency-free approval rule. Kept separate from persistence so it can
/// be unit tested exhaustively, including DST boundaries.
/// </summary>
public interface ITransactionApprovalEvaluator
{
    ApprovalDecision Evaluate(string timeZoneId, DateTimeOffset submittedAt);
}
