using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Issues signed JWT access tokens for authenticated users.
/// </summary>
public interface ITokenService
{
    /// <summary>Creates a signed token and returns it with its UTC expiry.</summary>
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}
