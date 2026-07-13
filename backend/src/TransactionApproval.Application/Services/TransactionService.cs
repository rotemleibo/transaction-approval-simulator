using System.Text.Json;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Application.Approval;
using TransactionApproval.Application.Common.Exceptions;
using TransactionApproval.Application.DTOs;
using TransactionApproval.Application.Events;
using TransactionApproval.Domain.Entities;
using TransactionApproval.Domain.Enums;

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

        await _transactions.AddAsync(transaction, new[] { BuildOutboxMessage(transaction) }, cancellationToken);

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

    private static OutboxMessage BuildOutboxMessage(Transaction transaction)
    {
        TransactionEvent @event = transaction.Status switch
        {
            TransactionStatus.Approved => new TransactionApprovedEvent(
                transaction.Id,
                transaction.CreatedAtUtc,
                transaction.RegionCode,
                transaction.RegionName),
            TransactionStatus.Rejected => new TransactionRejectedEvent(
                transaction.Id,
                transaction.CreatedAtUtc,
                transaction.RegionCode,
                transaction.RegionName),
            _ => throw new InvalidOperationException($"Unsupported transaction status '{transaction.Status}'.")
        };

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = @event.EventType,
            Payload = JsonSerializer.Serialize(@event),
            OccurredOnUtc = transaction.CreatedAtUtc,
            AvailableAtUtc = transaction.CreatedAtUtc
        };
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
