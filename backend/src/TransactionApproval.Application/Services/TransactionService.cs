using TransactionApproval.Application.Abstractions;
using TransactionApproval.Application.Approval;
using TransactionApproval.Application.Common.Exceptions;
using TransactionApproval.Application.DTOs;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Application.Services;

public class TransactionService : ITransactionService
{
    private const int MaxPageSize = 20;

    private readonly IRegionRepository _regions;
    private readonly ITransactionRepository _transactions;
    private readonly ITransactionApprovalEvaluator _evaluator;
    private readonly IClock _clock;

    public TransactionService(
        IRegionRepository regions,
        ITransactionRepository transactions,
        ITransactionApprovalEvaluator evaluator,
        IClock clock)
    {
        _regions = regions;
        _transactions = transactions;
        _evaluator = evaluator;
        _clock = clock;
    }

    public async Task<SimulateTransactionResponse> SimulateAsync(
        SimulateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var region = await _regions.GetByCodeAsync(request.RegionCode, cancellationToken)
            ?? throw new NotFoundException($"Region '{request.RegionCode}' is not supported.");

        ApprovalDecision decision = _evaluator.Evaluate(region.TimeZoneId, request.SubmittedAt);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            RegionCode = region.Code,
            RegionName = region.Name,
            TimeZoneId = region.TimeZoneId,
            SubmittedUtc = request.SubmittedAt.UtcDateTime,
            LocalTransactionTime = decision.LocalTransactionTime,
            Status = decision.Status,
            CreatedAtUtc = _clock.UtcNow
        };

        await _transactions.AddAsync(transaction, cancellationToken);

        return new SimulateTransactionResponse(
            transaction.Id,
            transaction.RegionCode,
            transaction.RegionName,
            transaction.TimeZoneId,
            transaction.SubmittedUtc,
            transaction.LocalTransactionTime,
            transaction.Status,
            decision.Reason);
    }

    public async Task<PagedResult<ApprovedTransactionDto>> GetApprovedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var (items, totalCount) = await _transactions.GetApprovedAsync(page, pageSize, cancellationToken);

        var dtos = items
            .Select(t => new ApprovedTransactionDto(
                t.Id,
                t.RegionCode,
                t.RegionName,
                t.TimeZoneId,
                t.SubmittedUtc,
                t.LocalTransactionTime,
                t.CreatedAtUtc))
            .ToList();

        return new PagedResult<ApprovedTransactionDto>(dtos, page, pageSize, totalCount);
    }
}
