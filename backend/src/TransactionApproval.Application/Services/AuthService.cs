using TransactionApproval.Application.Abstractions;
using TransactionApproval.Application.Common.Exceptions;
using TransactionApproval.Application.DTOs.Auth;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IClock _clock;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
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
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAtUtc = _clock.UtcNow
        };

        await _users.AddAsync(user, cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var user = await _users.GetByUsernameAsync(username, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
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
