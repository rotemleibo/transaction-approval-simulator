using TransactionApproval.Application.DTOs.Auth;

namespace TransactionApproval.Application.Services;

/// <summary>Handles account creation and login, returning signed JWTs.</summary>
public interface IAuthService
{
    Task<AuthResponse> SignupAsync(SignupRequest request, CancellationToken cancellationToken);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
