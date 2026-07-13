using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.LastError).HasMaxLength(2000);
        builder.Property(m => m.AvailableAtUtc).IsRequired();

        // Optimizes claiming pending messages for processing.
        builder.HasIndex(m => new { m.ProcessedOnUtc, m.DeadLetteredAtUtc, m.AvailableAtUtc, m.LeasedUntilUtc });
    }
}
