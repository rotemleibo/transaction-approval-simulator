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
        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException ex)
        {
            throw new InvalidOperationException(
                $"Time zone '{timeZoneId}' is not available on this system.", ex);
        }

        // Convert the absolute instant to the region's wall-clock time.
        DateTimeOffset localOffset = TimeZoneInfo.ConvertTime(submittedAt, timeZone);
        DateTime localTime = localOffset.DateTime;
        TimeOnly localTimeOfDay = TimeOnly.FromDateTime(localTime);

        bool withinBankingHours =
            localTimeOfDay >= _bankingHours.Open &&
            localTimeOfDay < _bankingHours.Close;

        TransactionStatus status = withinBankingHours
            ? TransactionStatus.Approved
            : TransactionStatus.Rejected;

        string reason = withinBankingHours
            ? $"Local time {localTime:HH:mm} is within banking hours " +
              $"({_bankingHours.Open:HH\\:mm}\u2013{_bankingHours.Close:HH\\:mm})."
            : $"Local time {localTime:HH:mm} is outside banking hours " +
              $"({_bankingHours.Open:HH\\:mm}\u2013{_bankingHours.Close:HH\\:mm}).";

        return new ApprovalDecision(localTime, status, reason);
    }
}
