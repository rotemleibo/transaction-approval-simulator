namespace TransactionApproval.Application.DTOs.Auth;

/// <summary>Credentials for creating a new account.</summary>
public record SignupRequest(string Username, string Password);

/// <summary>Credentials for logging in.</summary>
public record LoginRequest(string Username, string Password);

/// <summary>Successful authentication result.</summary>
public record AuthResponse(string Username, string Token, DateTime ExpiresAtUtc);
