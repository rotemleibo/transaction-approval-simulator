using TransactionApproval.Domain.Enums;

namespace TransactionApproval.Domain.Entities;

/// <summary>
/// A persisted transaction simulation. Both approved and rejected results are
/// stored. The absolute instant is kept in UTC; the region-local time that the
/// approval decision was based on is stored alongside it for auditability.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }

    /// <summary>Region code the simulation was run against.</summary>
    public required string RegionCode { get; set; }

    /// <summary>Denormalized region name captured at simulation time.</summary>
    public required string RegionName { get; set; }

    /// <summary>IANA time zone id used for the conversion.</summary>
    public required string TimeZoneId { get; set; }

    /// <summary>The absolute instant submitted, stored in UTC.</summary>
    public DateTime SubmittedUtc { get; set; }

    /// <summary>The submitted instant expressed in the region's local time.</summary>
    public DateTime LocalTransactionTime { get; set; }

    /// <summary>Approval outcome computed on the server.</summary>
    public TransactionStatus Status { get; set; }

    /// <summary>When this record was created, in UTC.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Navigation to the owning region.</summary>
    public Region? Region { get; set; }
}
