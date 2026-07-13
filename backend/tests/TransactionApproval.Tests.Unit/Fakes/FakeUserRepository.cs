using TransactionApproval.Application.Abstractions;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Tests.Unit.Fakes;

public sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<string, User> _store = new(StringComparer.OrdinalIgnoreCase);

    public List<User> Added { get; } = [];

    public FakeUserRepository(params User[] seed)
    {
        foreach (var user in seed)
            _store[user.Username] = user;
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        => Task.FromResult(_store.TryGetValue(username, out var user) ? user : null);

    public Task<bool> ExistsAsync(string username, CancellationToken cancellationToken)
        => Task.FromResult(_store.ContainsKey(username));

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _store[user.Username] = user;
        Added.Add(user);
        return Task.CompletedTask;
    }
}
