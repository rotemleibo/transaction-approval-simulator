namespace TransactionApproval.Domain.Entities;

/// <summary>
/// A supported banking region. The catalog is fixed and seeded, so approval
/// decisions always run against a known, stable IANA time zone identifier
/// rather than free-text user input.
/// </summary>
public class Region
{
    /// <summary>Stable business code, e.g. "IL", "US-East".</summary>
    public required string Code { get; set; }

    /// <summary>Human-readable display name, e.g. "Israel".</summary>
    public required string Name { get; set; }

    /// <summary>
    /// IANA time zone identifier, e.g. "Asia/Jerusalem". .NET 6+ resolves both
    /// IANA and Windows ids across platforms, so IANA keeps us portable.
    /// </summary>
    public required string TimeZoneId { get; set; }
}
