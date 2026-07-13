using TransactionApproval.Application.Abstractions;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Tests.Unit.Fakes;

public sealed class FakeTokenService : ITokenService
{
    public DateTime ExpiresAtUtc { get; } = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
        => ($"token:{user.Username}", ExpiresAtUtc);
}
