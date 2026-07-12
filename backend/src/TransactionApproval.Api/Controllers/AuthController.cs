using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TransactionApproval.Application.DTOs.Auth;
using TransactionApproval.Application.Services;

namespace TransactionApproval.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<SignupRequest> _signupValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        IAuthService authService,
        IValidator<SignupRequest> signupValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _authService = authService;
        _signupValidator = signupValidator;
        _loginValidator = loginValidator;
    }

    /// <summary>Creates a new account and returns a signed JWT.</summary>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Signup(
        [FromBody] SignupRequest request,
        CancellationToken cancellationToken)
    {
        await _signupValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _authService.SignupAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Authenticates a user and returns a signed JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }
}
