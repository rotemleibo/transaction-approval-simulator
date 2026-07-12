using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionApproval.Application.DTOs;
using TransactionApproval.Application.Services;

namespace TransactionApproval.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IValidator<SimulateTransactionRequest> _validator;

    public TransactionsController(
        ITransactionService transactionService,
        IValidator<SimulateTransactionRequest> validator)
    {
        _transactionService = transactionService;
        _validator = validator;
    }

    /// <summary>Simulates a transaction: validates, evaluates approval, and persists it.</summary>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(SimulateTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SimulateTransactionResponse>> Simulate(
        [FromBody] SimulateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _transactionService.SimulateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns approved transactions only, newest first, paginated.</summary>
    [HttpGet("approved")]
    [ProducesResponseType(typeof(PagedResult<ApprovedTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ApprovedTransactionDto>>> GetApproved(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _transactionService.GetApprovedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }
}
