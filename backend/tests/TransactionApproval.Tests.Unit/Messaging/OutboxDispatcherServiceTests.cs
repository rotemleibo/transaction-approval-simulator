using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Application.Events;
using TransactionApproval.Domain.Entities;
using TransactionApproval.Infrastructure.Messaging;

namespace TransactionApproval.Tests.Unit.Messaging;

public class OutboxDispatcherServiceTests
{
    [Fact]
    public async Task ExecuteAsync_PublishSucceeds_MarksMessageProcessed()
    {
        var now = new DateTime(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);
        var repository = new FakeOutboxRepository([CreateMessage()]);
        var publisher = new FakePublisher(throwOnPublish: false);
        var serializer = new FakeSerializer();
        var dispatcher = CreateDispatcher(repository, serializer, publisher, now, maxRetryCount: 5);

        await dispatcher.StartAsync(CancellationToken.None);
        await repository.Completion.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await dispatcher.StopAsync(CancellationToken.None);

        Assert.Equal(1, repository.MarkProcessedCount);
        Assert.Equal(0, repository.MarkFailedCount);
        Assert.Equal(0, repository.MarkDeadLetteredCount);
    }

    [Fact]
    public async Task ExecuteAsync_PublishFailsBeforeMaxRetry_MarksFailedWithBackoff()
    {
        var now = new DateTime(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);
        var repository = new FakeOutboxRepository([CreateMessage(attempts: 0)]);
        var publisher = new FakePublisher(throwOnPublish: true);
        var serializer = new FakeSerializer();
        var dispatcher = CreateDispatcher(repository, serializer, publisher, now, maxRetryCount: 5);

        await dispatcher.StartAsync(CancellationToken.None);
        await repository.Completion.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await dispatcher.StopAsync(CancellationToken.None);

        Assert.Equal(0, repository.MarkProcessedCount);
        Assert.Equal(1, repository.MarkFailedCount);
        Assert.Equal(0, repository.MarkDeadLetteredCount);
        Assert.Equal(now + TimeSpan.FromSeconds(2), repository.LastNextAttemptAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_PublishFailsAtMaxRetry_MarksDeadLettered()
    {
        var now = new DateTime(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);
        var repository = new FakeOutboxRepository([CreateMessage(attempts: 0)]);
        var publisher = new FakePublisher(throwOnPublish: true);
        var serializer = new FakeSerializer();
        var dispatcher = CreateDispatcher(repository, serializer, publisher, now, maxRetryCount: 1);

        await dispatcher.StartAsync(CancellationToken.None);
        await repository.Completion.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await dispatcher.StopAsync(CancellationToken.None);

        Assert.Equal(0, repository.MarkProcessedCount);
        Assert.Equal(0, repository.MarkFailedCount);
        Assert.Equal(1, repository.MarkDeadLetteredCount);
    }

    private static OutboxDispatcherService CreateDispatcher(
        FakeOutboxRepository repository,
        FakeSerializer serializer,
        FakePublisher publisher,
        DateTime nowUtc,
        int maxRetryCount)
    {
        var services = new ServiceCollection();
        services.AddScoped<IOutboxRepository>(_ => repository);
        services.AddScoped<IOutboxEventSerializer>(_ => serializer);
        services.AddScoped<IEventPublisher>(_ => publisher);

        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var options = Options.Create(new OutboxDispatcherOptions
        {
            PollingIntervalSeconds = 60,
            BatchSize = 20,
            MaxRetryCount = maxRetryCount,
            LeaseDuration = TimeSpan.FromSeconds(30),
            BaseBackoff = TimeSpan.FromSeconds(2),
            MaxBackoff = TimeSpan.FromMinutes(1)
        });

        return new OutboxDispatcherService(
            scopeFactory,
            options,
            new FakeTimeProvider(nowUtc),
            NullLogger<OutboxDispatcherService>.Instance);
    }

    private static OutboxMessage CreateMessage(int attempts = 0) => new()
    {
        Id = Guid.NewGuid(),
        Type = EventTypes.TransactionApproved,
        Payload = "{}",
        OccurredOnUtc = new DateTime(2026, 7, 13, 11, 59, 0, DateTimeKind.Utc),
        AvailableAtUtc = new DateTime(2026, 7, 13, 11, 59, 0, DateTimeKind.Utc),
        Attempts = attempts
    };

    private sealed class FakeOutboxRepository : IOutboxRepository
    {
        private readonly IReadOnlyList<OutboxMessage> _messages;

        public FakeOutboxRepository(IReadOnlyList<OutboxMessage> messages)
        {
            _messages = messages;
        }

        public int MarkProcessedCount { get; private set; }
        public int MarkFailedCount { get; private set; }
        public int MarkDeadLetteredCount { get; private set; }
        public DateTime? LastNextAttemptAtUtc { get; private set; }
        public TaskCompletionSource Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(
            int batchSize,
            DateTime nowUtc,
            DateTime leasedUntilUtc,
            CancellationToken cancellationToken) =>
            Task.FromResult(_messages);

        public Task MarkProcessedAsync(Guid id, DateTime processedOnUtc, CancellationToken cancellationToken)
        {
            MarkProcessedCount += 1;
            Completion.TrySetResult();
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(
            Guid id,
            string error,
            DateTime nextAttemptAtUtc,
            CancellationToken cancellationToken)
        {
            MarkFailedCount += 1;
            LastNextAttemptAtUtc = nextAttemptAtUtc;
            Completion.TrySetResult();
            return Task.CompletedTask;
        }

        public Task MarkDeadLetteredAsync(
            Guid id,
            DateTime deadLetteredAtUtc,
            string reason,
            CancellationToken cancellationToken)
        {
            MarkDeadLetteredCount += 1;
            Completion.TrySetResult();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSerializer : IOutboxEventSerializer
    {
        public (string EventType, string Payload) Serialize(TransactionEvent @event) =>
            (@event.EventType, "{}");

        public TransactionEvent Deserialize(string eventType, string payload) =>
            new TransactionApprovedEvent(Guid.NewGuid(), DateTime.UtcNow, "IL", "Israel");
    }

    private sealed class FakePublisher : IEventPublisher
    {
        private readonly bool _throwOnPublish;

        public FakePublisher(bool throwOnPublish)
        {
            _throwOnPublish = throwOnPublish;
        }

        public Task PublishAsync(TransactionEvent @event, CancellationToken cancellationToken)
        {
            if (_throwOnPublish)
            {
                throw new InvalidOperationException("Synthetic publish failure.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTime utcNow)
        {
            _utcNow = new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
