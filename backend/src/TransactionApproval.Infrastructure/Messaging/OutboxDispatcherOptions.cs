namespace TransactionApproval.Infrastructure.Messaging;

/// <summary>
/// Configuration for the background outbox dispatcher.
/// </summary>
public class OutboxDispatcherOptions
{
    public const string SectionName = "Outbox";

    public int PollingIntervalSeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 20;

    public int MaxRetryCount { get; set; } = 5;

    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan BaseBackoff { get; set; } = TimeSpan.FromSeconds(2);

    public TimeSpan MaxBackoff { get; set; } = TimeSpan.FromMinutes(1);
}
