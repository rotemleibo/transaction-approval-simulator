using Microsoft.AspNetCore.Identity;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Tests.Unit.Fakes;

/// <summary>
/// Test double for password hashing. Keeps auth tests readable without using
/// the concrete runtime hashing implementation.
/// </summary>
public sealed class FakePasswordHasher : IPasswordHasher<User>
{
    public string HashPassword(User user, string password) => $"hashed:{password}";

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword) =>
        hashedPassword == $"hashed:{providedPassword}"
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
}
