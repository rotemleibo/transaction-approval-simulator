namespace TransactionApproval.Application.Approval;

/// <summary>
/// Configurable banking-hours window used by the approval rule. A transaction
/// is approved when the region-local time is in <c>[OpenHour, CloseHour)</c>.
/// </summary>
public class BankingHoursOptions
{
    public const string SectionName = "BankingHours";

    /// <summary>Inclusive opening hour (local), default 08:00.</summary>
    public int OpenHour { get; set; } = 8;

    /// <summary>Exclusive closing hour (local), default 18:00.</summary>
    public int CloseHour { get; set; } = 18;
}
