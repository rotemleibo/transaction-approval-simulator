using Microsoft.Extensions.Options;
using TransactionApproval.Domain.Enums;

namespace TransactionApproval.Application.Approval;

/// <summary>
/// Determines approval by converting the submitted absolute instant into the
/// region's local time and checking it against banking hours. DST is handled
/// automatically by <see cref="TimeZoneInfo"/>; no manual offsets are used.
/// </summary>
public class TransactionApprovalEvaluator : ITransactionApprovalEvaluator
{
    private readonly BankingHoursOptions _bankingHours;

    public TransactionApprovalEvaluator(IOptions<BankingHoursOptions> bankingHours)
    {
        _bankingHours = bankingHours.Value;
    }

    public ApprovalDecision Evaluate(string timeZoneId, DateTimeOffset submittedAt)
    {
        // .NET 6+ resolves both IANA (e.g. "Asia/Jerusalem") and Windows ids on
        // every platform, so a seeded IANA id works on Linux and Windows alike.
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // Convert the absolute instant to the region's wall-clock time.
        DateTimeOffset localOffset = TimeZoneInfo.ConvertTime(submittedAt, timeZone);
        DateTime localTime = localOffset.DateTime;

        bool withinBankingHours =
            localTime.Hour >= _bankingHours.OpenHour &&
            localTime.Hour < _bankingHours.CloseHour;

        TransactionStatus status = withinBankingHours
            ? TransactionStatus.Approved
            : TransactionStatus.Rejected;

        string reason = withinBankingHours
            ? $"Local time {localTime:HH:mm} is within banking hours " +
              $"({_bankingHours.OpenHour:00}:00\u2013{_bankingHours.CloseHour:00}:00)."
            : $"Local time {localTime:HH:mm} is outside banking hours " +
              $"({_bankingHours.OpenHour:00}:00\u2013{_bankingHours.CloseHour:00}:00).";

        return new ApprovalDecision(localTime, status, reason);
    }
}
