using TransactionApproval.Application.Approval;

namespace TransactionApproval.Tests.Unit.Fakes;

public sealed class FakeEvaluator : ITransactionApprovalEvaluator
{
    private readonly ApprovalDecision _decision;

    public FakeEvaluator(ApprovalDecision decision) => _decision = decision;

    public ApprovalDecision Evaluate(string timeZoneId, DateTimeOffset submittedAt) => _decision;
}
