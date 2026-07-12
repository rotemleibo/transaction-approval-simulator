namespace TransactionApproval.Domain.Entities;

/// <summary>
/// An application user for the JWT auth flow. Passwords are never stored in
/// plain text; only a salted hash produced by the password hasher is kept.
/// </summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>Unique, case-insensitive login name.</summary>
    public required string Username { get; set; }

    /// <summary>Salted password hash.</summary>
    public required string PasswordHash { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
