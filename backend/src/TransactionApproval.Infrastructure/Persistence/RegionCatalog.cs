namespace TransactionApproval.Infrastructure.Persistence;

/// <summary>
/// Single source of truth for the supported region catalog. Referenced by the
/// EF seed so the same data ships with every migration and container.
/// </summary>
public static class RegionCatalog
{
    public static readonly (string Code, string Name, string TimeZoneId)[] Regions =
    {
        ("IL", "Israel", "Asia/Jerusalem"),
        ("FR", "France", "Europe/Paris"),
        ("US-East", "United States (East)", "America/New_York"),
        ("US-West", "United States (West)", "America/Los_Angeles"),
        ("JP", "Japan", "Asia/Tokyo"),
        ("GB", "United Kingdom", "Europe/London"),
        ("DE", "Germany", "Europe/Berlin"),
        ("CY", "Cyprus", "Asia/Nicosia"),
        ("IT", "Italy", "Europe/Rome"),
        ("AU", "Australia (Sydney)", "Australia/Sydney"),
    };
}
