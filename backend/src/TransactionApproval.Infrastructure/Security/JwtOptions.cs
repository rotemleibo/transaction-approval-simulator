namespace TransactionApproval.Infrastructure.Security;

/// <summary>JWT signing and validation settings, bound from configuration.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Symmetric signing key. Must be at least 32 characters (256 bits).</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
