using System.Text.Json;
using TransactionApproval.Application.Approval;
using TransactionApproval.Application.Common.Exceptions;
using TransactionApproval.Application.DTOs;
using TransactionApproval.Application.Events;
using TransactionApproval.Application.Services;
using TransactionApproval.Domain.Entities;
using TransactionApproval.Domain.Enums;
using TransactionApproval.Tests.Unit.Fakes;

namespace TransactionApproval.Tests.Unit.Services;

public class TransactionServiceTests
{
    #region Helpers

    private static Region MakeRegion() => new()
    {
        Code = "IL",
        Name = "Israel",
        TimeZoneId = "Asia/Jerusalem"
    };

    private static ApprovalDecision MakeDecision(TransactionStatus status) => new(
        LocalTransactionTime: new DateTime(2026, 1, 1, 14, 0, 0),
        Status: status,
        Reason: status == TransactionStatus.Approved ? "Within banking hours" : "Outside banking hours");

    private static TransactionService CreateService(
        Region? region,
        ApprovalDecision decision,
        FakeTransactionRepository? transactionRepository = null) => new(
            new FakeRegionRepository(region),
            transactionRepository ?? new FakeTransactionRepository(),
            new FakeEvaluator(decision),
            new FakeClock());

    #endregion

    #region SimulateAsync

    [Fact]
    public async Task SimulateAsync_RegionNotFound_ThrowsNotFoundException()
    {
        var service = CreateService(region: null, decision: MakeDecision(TransactionStatus.Approved));
        var request = new SimulateTransactionRequest("XX", DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.SimulateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SimulateAsync_ApprovedDecision_ReturnsMappedResponse()
    {
        var decision = MakeDecision(TransactionStatus.Approved);
        var transactionRepository = new FakeTransactionRepository();
        var service = CreateService(MakeRegion(), decision, transactionRepository);
        var submittedAt = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var request = new SimulateTransactionRequest("IL", submittedAt);

        var response = await service.SimulateAsync(request, CancellationToken.None);

        Assert.Equal(TransactionStatus.Approved, response.Status);
        Assert.Equal("IL", response.RegionCode);
        Assert.Equal("Israel", response.RegionName);
        Assert.Equal("Asia/Jerusalem", response.TimeZoneId);
        Assert.Equal(submittedAt.UtcDateTime, response.SubmittedUtc);
        Assert.Equal(decision.LocalTransactionTime, response.LocalTransactionTime);
        Assert.Equal(decision.Reason, response.Reason);
    }

    [Fact]
    public async Task SimulateAsync_RejectedDecision_ReturnsMappedResponse()
    {
        var decision = MakeDecision(TransactionStatus.Rejected);
        var service = CreateService(MakeRegion(), decision);
        var request = new SimulateTransactionRequest("IL", DateTimeOffset.UtcNow);

        var response = await service.SimulateAsync(request, CancellationToken.None);

        Assert.Equal(TransactionStatus.Rejected, response.Status);
        Assert.Equal(decision.Reason, response.Reason);
    }

    [Fact]
    public async Task SimulateAsync_PersistsTransactionWithCorrectFields()
    {
        var decision = MakeDecision(TransactionStatus.Approved);
        var transactionRepository = new FakeTransactionRepository();
        var clock = new FakeClock();
        var service = new TransactionService(
            new FakeRegionRepository(MakeRegion()),
            transactionRepository,
            new FakeEvaluator(decision),
            clock);
        var submittedAt = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

        await service.SimulateAsync(new SimulateTransactionRequest("IL", submittedAt), CancellationToken.None);

        var saved = Assert.Single(transactionRepository.Transactions);
        Assert.Equal("IL", saved.RegionCode);
        Assert.Equal("Israel", saved.RegionName);
        Assert.Equal("Asia/Jerusalem", saved.TimeZoneId);
        Assert.Equal(submittedAt.UtcDateTime, saved.SubmittedUtc);
        Assert.Equal(decision.LocalTransactionTime, saved.LocalTransactionTime);
        Assert.Equal(decision.Status, saved.Status);
    }

    [Fact]
    public async Task SimulateAsync_ApprovedDecision_ProducesOutboxMessage_WithExpectedEventFields()
    {
        var decision = MakeDecision(TransactionStatus.Approved);
        var transactionRepository = new FakeTransactionRepository();
        var service = CreateService(MakeRegion(), decision, transactionRepository);
        var submittedAt = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

        var response = await service.SimulateAsync(
            new SimulateTransactionRequest("IL", submittedAt), CancellationToken.None);

        var outboxMessage = Assert.Single(transactionRepository.OutboxMessages);
        Assert.Equal(EventTypes.TransactionApproved, outboxMessage.Type);

        var approvedEvent = JsonSerializer.Deserialize<TransactionApprovedEvent>(outboxMessage.Payload);
        Assert.NotNull(approvedEvent);
        Assert.Equal(EventTypes.TransactionApproved, approvedEvent!.EventType);
        Assert.Equal(response.Id, approvedEvent.TransactionId);
        Assert.Equal("IL", approvedEvent.RegionCode);
        Assert.Equal("Israel", approvedEvent.RegionName);
        Assert.NotEqual(default, approvedEvent.TimestampUtc);
    }

    [Fact]
    public async Task SimulateAsync_RejectedDecision_ProducesOutboxMessage_WithExpectedEventFields()
    {
        var decision = MakeDecision(TransactionStatus.Rejected);
        var transactionRepository = new FakeTransactionRepository();
        var service = CreateService(MakeRegion(), decision, transactionRepository);
        var submittedAt = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

        var response = await service.SimulateAsync(
            new SimulateTransactionRequest("IL", submittedAt), CancellationToken.None);

        var outboxMessage = Assert.Single(transactionRepository.OutboxMessages);
        Assert.Equal(EventTypes.TransactionRejected, outboxMessage.Type);

        var rejectedEvent = JsonSerializer.Deserialize<TransactionRejectedEvent>(outboxMessage.Payload);
        Assert.NotNull(rejectedEvent);
        Assert.Equal(EventTypes.TransactionRejected, rejectedEvent!.EventType);
        Assert.Equal(response.Id, rejectedEvent.TransactionId);
        Assert.Equal("IL", rejectedEvent.RegionCode);
        Assert.Equal("Israel", rejectedEvent.RegionName);
        Assert.NotEqual(default, rejectedEvent.TimestampUtc);
    }

    #endregion

    #region GetApprovedAsync

    [Fact]
    public async Task GetApprovedAsync_ReturnsMappedDtos()
    {
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            RegionCode = "IL",
            RegionName = "Israel",
            TimeZoneId = "Asia/Jerusalem",
            SubmittedUtc = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            LocalTransactionTime = new DateTime(2026, 1, 1, 12, 0, 0),
            Status = TransactionStatus.Approved,
            CreatedAtUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };
        var transactionRepository = new FakeTransactionRepository([tx], totalCount: 1);
        var service = new TransactionService(
            new FakeRegionRepository(null), transactionRepository, new FakeEvaluator(MakeDecision(TransactionStatus.Approved)), new FakeClock());

        var result = await service.GetApprovedAsync(1, 10, CancellationToken.None);

        var dto = Assert.Single(result.Items);
        Assert.Equal(tx.Id, dto.Id);
        Assert.Equal(tx.RegionCode, dto.RegionCode);
        Assert.Equal(tx.RegionName, dto.RegionName);
        Assert.Equal(tx.TimeZoneId, dto.TimeZoneId);
        Assert.Equal(tx.SubmittedUtc, dto.SubmittedUtc);
        Assert.Equal(tx.LocalTransactionTime, dto.LocalTransactionTime);
        Assert.Equal(tx.CreatedAtUtc, dto.CreatedAtUtc);
    }

    [Fact]
    public async Task GetApprovedAsync_ReturnsPaginationMetadata()
    {
        var transactionRepository = new FakeTransactionRepository([], totalCount: 50);
        var service = new TransactionService(
            new FakeRegionRepository(null), transactionRepository, new FakeEvaluator(MakeDecision(TransactionStatus.Approved)), new FakeClock());

        var result = await service.GetApprovedAsync(page: 2, pageSize: 10, CancellationToken.None);

        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(50, result.TotalCount);
        Assert.Equal(5, result.TotalPages);
    }

    [Theory]
    [InlineData(0, 1)]   // below minimum -> clamped to 1
    [InlineData(-5, 1)]  // negative -> clamped to 1
    public async Task GetApprovedAsync_PageBelowOne_ClampedToOne(int inputPage, int expectedPage)
    {
        var transactionRepository = new FakeTransactionRepository();
        var service = new TransactionService(
            new FakeRegionRepository(null), transactionRepository, new FakeEvaluator(MakeDecision(TransactionStatus.Approved)), new FakeClock());

        var result = await service.GetApprovedAsync(inputPage, 10, CancellationToken.None);

        Assert.Equal(expectedPage, result.Page);
    }

    [Theory]
    [InlineData(0, 1)]    // below minimum -> clamped to 1
    [InlineData(21, 20)]  // above MaxPageSize (20) -> clamped to 20
    [InlineData(100, 20)] // well above MaxPageSize -> clamped to 20
    public async Task GetApprovedAsync_PageSizeOutOfRange_Clamped(int inputPageSize, int expectedPageSize)
    {
        var transactionRepository = new FakeTransactionRepository();
        var service = new TransactionService(
            new FakeRegionRepository(null), transactionRepository, new FakeEvaluator(MakeDecision(TransactionStatus.Approved)), new FakeClock());

        var result = await service.GetApprovedAsync(1, inputPageSize, CancellationToken.None);

        Assert.Equal(expectedPageSize, result.PageSize);
    }

    #endregion
}
