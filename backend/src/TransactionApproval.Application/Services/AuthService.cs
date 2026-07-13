using Microsoft.AspNetCore.Identity;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Application.Common.Exceptions;
using TransactionApproval.Application.DTOs.Auth;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IClock _clock;

    public AuthService(
        IUserRepository users,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService,
        IClock clock)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _clock = clock;
    }

    public async Task<AuthResponse> SignupAsync(SignupRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();

        if (await _users.ExistsAsync(username, cancellationToken))
        {
            throw new ConflictException($"Username '{username}' is already taken.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            CreatedAtUtc = _clock.UtcNow,
            PasswordHash = string.Empty
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _users.AddAsync(user, cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var user = await _users.GetByUsernameAsync(username, cancellationToken);

        if (user is null)
        {
            throw new AuthenticationException("Invalid username or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result is PasswordVerificationResult.Failed)
        {
            // Same message for both cases to avoid leaking which usernames exist.
            throw new AuthenticationException("Invalid username or password.");
        }

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var (token, expiresAtUtc) = _tokenService.CreateToken(user);
        return new AuthResponse(user.Username, token, expiresAtUtc);
    }
}
