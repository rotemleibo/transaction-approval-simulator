using TransactionApproval.Application.DTOs;

namespace TransactionApproval.Application.Services;

/// <summary>Orchestrates simulation (evaluate + persist) and approved queries.</summary>
public interface ITransactionService
{
    Task<SimulateTransactionResponse> SimulateAsync(
        SimulateTransactionRequest request,
        CancellationToken cancellationToken);

    Task<PagedResult<ApprovedTransactionDto>> GetApprovedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
