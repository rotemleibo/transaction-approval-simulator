using Microsoft.AspNetCore.Identity;
using TransactionApproval.Application.Common.Exceptions;
using TransactionApproval.Application.DTOs.Auth;
using TransactionApproval.Application.Services;
using TransactionApproval.Domain.Entities;
using TransactionApproval.Tests.Unit.Fakes;

namespace TransactionApproval.Tests.Unit.Services;

public class AuthServiceTests
{
    #region Helpers

    private static User MakeUser(string username = "alice", string password = "secret")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = string.Empty,
            CreatedAtUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        user.PasswordHash = new FakePasswordHasher().HashPassword(user, password);
        return user;
    }

    private static AuthService CreateService(
        FakeUserRepository? users = null,
        IPasswordHasher<User>? hasher = null,
        FakeTokenService? tokenService = null,
        FakeClock? clock = null) => new(
            users ?? new FakeUserRepository(),
            hasher ?? new FakePasswordHasher(),
            tokenService ?? new FakeTokenService(),
            clock ?? new FakeClock());

    #endregion

    #region SignupAsync

    [Fact]
    public async Task SignupAsync_UsernameAlreadyExists_ThrowsConflictException()
    {
        var users = new FakeUserRepository(MakeUser("alice"));
        var service = CreateService(users);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.SignupAsync(new SignupRequest("alice", "password"), CancellationToken.None));
    }

    [Fact]
    public async Task SignupAsync_UsernameAlreadyExists_IsCaseInsensitive()
    {
        var users = new FakeUserRepository(MakeUser("alice"));
        var service = CreateService(users);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.SignupAsync(new SignupRequest("ALICE", "password"), CancellationToken.None));
    }

    [Fact]
    public async Task SignupAsync_NewUser_PersistsWithTrimmedUsername()
    {
        var users = new FakeUserRepository();
        var service = CreateService(users);

        await service.SignupAsync(new SignupRequest("  alice  ", "secret"), CancellationToken.None);

        var saved = Assert.Single(users.Added);
        Assert.Equal("alice", saved.Username);
    }

    [Fact]
    public async Task SignupAsync_NewUser_PersistsHashedPassword()
    {
        var users = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        var service = CreateService(users, hasher);

        await service.SignupAsync(new SignupRequest("alice", "secret"), CancellationToken.None);

        var saved = Assert.Single(users.Added);
        Assert.Equal(hasher.HashPassword(saved, "secret"), saved.PasswordHash);
    }

    [Fact]
    public async Task SignupAsync_NewUser_PersistsClockTimestamp()
    {
        var users = new FakeUserRepository();
        var clock = new FakeClock();
        var service = CreateService(users, clock: clock);

        await service.SignupAsync(new SignupRequest("alice", "secret"), CancellationToken.None);

        var saved = Assert.Single(users.Added);
        Assert.Equal(clock.UtcNow, saved.CreatedAtUtc);
    }

    [Fact]
    public async Task SignupAsync_NewUser_ReturnsCorrectAuthResponse()
    {
        var tokenService = new FakeTokenService();
        var service = CreateService(tokenService: tokenService);

        var response = await service.SignupAsync(new SignupRequest("alice", "secret"), CancellationToken.None);

        Assert.Equal("alice", response.Username);
        Assert.Equal($"token:alice", response.Token);
        Assert.Equal(tokenService.ExpiresAtUtc, response.ExpiresAtUtc);
    }

    [Fact]
    public async Task SignupAsync_NewUser_AssignsNonEmptyId()
    {
        var users = new FakeUserRepository();
        var service = CreateService(users);

        await service.SignupAsync(new SignupRequest("alice", "secret"), CancellationToken.None);

        var saved = Assert.Single(users.Added);
        Assert.NotEqual(Guid.Empty, saved.Id);
    }

    #endregion

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsAuthenticationException()
    {
        var service = CreateService(new FakeUserRepository());

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            service.LoginAsync(new LoginRequest("alice", "secret"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsAuthenticationException()
    {
        var users = new FakeUserRepository(MakeUser("alice", "secret"));
        var service = CreateService(users);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            service.LoginAsync(new LoginRequest("alice", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_AndWrongPassword_SameExceptionMessage()
    {
        var users = new FakeUserRepository(MakeUser("alice", "secret"));
        var service = CreateService(users);

        var notFound = await Assert.ThrowsAsync<AuthenticationException>(() =>
            service.LoginAsync(new LoginRequest("unknown", "x"), CancellationToken.None));

        var wrongPassword = await Assert.ThrowsAsync<AuthenticationException>(() =>
            service.LoginAsync(new LoginRequest("alice", "wrong"), CancellationToken.None));

        Assert.Equal(notFound.Message, wrongPassword.Message);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsCorrectAuthResponse()
    {
        var tokenService = new FakeTokenService();
        var users = new FakeUserRepository(MakeUser("alice", "secret"));
        var service = CreateService(users, tokenService: tokenService);

        var response = await service.LoginAsync(new LoginRequest("alice", "secret"), CancellationToken.None);

        Assert.Equal("alice", response.Username);
        Assert.Equal("token:alice", response.Token);
        Assert.Equal(tokenService.ExpiresAtUtc, response.ExpiresAtUtc);
    }

    [Fact]
    public async Task LoginAsync_UsernameIsTrimmed()
    {
        var users = new FakeUserRepository(MakeUser("alice", "secret"));
        var service = CreateService(users);

        var response = await service.LoginAsync(new LoginRequest("  alice  ", "secret"), CancellationToken.None);

        Assert.Equal("alice", response.Username);
    }

    #endregion
}
