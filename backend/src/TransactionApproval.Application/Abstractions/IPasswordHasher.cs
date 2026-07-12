namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Hashes and verifies user passwords. Implementation lives in Infrastructure
/// so the algorithm can change without touching application logic.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
