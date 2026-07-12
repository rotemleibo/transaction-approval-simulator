using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Persistence for application users.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);
}
