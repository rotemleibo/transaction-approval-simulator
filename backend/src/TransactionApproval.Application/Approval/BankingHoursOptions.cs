namespace TransactionApproval.Application.Approval;

/// <summary>
/// Configurable banking-hours window used by the approval rule. A transaction
/// is approved when the region-local time is in <c>[Open, Close)</c>.
/// Supports minute-level precision (e.g. "08:30").
/// </summary>
public class BankingHoursOptions
{
    public const string SectionName = "BankingHours";

    /// <summary>Inclusive opening time (local), default 08:00.</summary>
    public TimeOnly Open { get; set; } = new(8, 0);

    /// <summary>Exclusive closing time (local), default 18:00.</summary>
    public TimeOnly Close { get; set; } = new(18, 0);
}
