using Microsoft.Extensions.Options;
using TransactionApproval.Application.Approval;
using TransactionApproval.Domain.Enums;

namespace TransactionApproval.Tests.Unit.Approval;

public class TransactionApprovalEvaluatorTests
{
    private static TransactionApprovalEvaluator CreateSubject(int openHour = 8, int closeHour = 18)
    {
        var options = Options.Create(new BankingHoursOptions
        {
            Open = new TimeOnly(openHour, 0),
            Close = new TimeOnly(closeHour, 0)
        });

        return new TransactionApprovalEvaluator(options);
    }

    [Theory]
    [InlineData("Europe/Paris", "2026-07-12T05:59:00Z", TransactionStatus.Rejected)] // 07:59 local
    [InlineData("Europe/Paris", "2026-07-12T06:00:00Z", TransactionStatus.Approved)] // 08:00 local
    [InlineData("Europe/Paris", "2026-07-12T15:59:00Z", TransactionStatus.Approved)] // 17:59 local
    [InlineData("Europe/Paris", "2026-07-12T16:00:00Z", TransactionStatus.Rejected)] // 18:00 local
    public void Evaluate_RespectsInclusiveOpen_ExclusiveClose(
        string timeZoneId,
        string submittedUtcIso,
        TransactionStatus expected)
    {
        var subject = CreateSubject();
        var submittedAt = DateTimeOffset.Parse(submittedUtcIso);

        var result = subject.Evaluate(timeZoneId, submittedAt);

        Assert.Equal(expected, result.Status);
    }

    [Fact]
    public void Evaluate_HandlesIsraelSummerTimeThroughTimeZoneInfo()
    {
        // July in Israel is DST (UTC+3). 05:00Z should map to 08:00 local.
        var subject = CreateSubject();
        var submittedAt = DateTimeOffset.Parse("2026-07-12T05:00:00Z");

        var result = subject.Evaluate("Asia/Jerusalem", submittedAt);

        Assert.Equal(8, result.LocalTransactionTime.Hour);
        Assert.Equal(TransactionStatus.Approved, result.Status);
    }

    [Fact]
    public void Evaluate_HandlesUsDstTransitionWithoutManualOffsets()
    {
        // 2026-03-08 is US spring-forward day (America/New_York).
        // 12:00Z maps to 08:00 local after DST shift and should be approved.
        var subject = CreateSubject();
        var submittedAt = DateTimeOffset.Parse("2026-03-08T12:00:00Z");

        var result = subject.Evaluate("America/New_York", submittedAt);

        Assert.Equal(8, result.LocalTransactionTime.Hour);
        Assert.Equal(TransactionStatus.Approved, result.Status);
    }

    [Fact]
    public void Evaluate_ThrowsInvalidOperation_ForUnknownTimeZone()
    {
        var subject = CreateSubject();

        var ex = Assert.Throws<InvalidOperationException>(
            () => subject.Evaluate("Not/A_Zone", DateTimeOffset.UtcNow));

        Assert.Contains("Not/A_Zone", ex.Message);
    }
}
