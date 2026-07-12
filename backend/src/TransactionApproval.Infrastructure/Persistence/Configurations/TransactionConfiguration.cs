using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.RegionCode).HasMaxLength(16).IsRequired();
        builder.Property(t => t.RegionName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.TimeZoneId).HasMaxLength(64).IsRequired();

        // Persist the enum as a readable string for easy inspection in the DB.
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasOne(t => t.Region)
            .WithMany()
            .HasForeignKey(t => t.RegionCode)
            .OnDelete(DeleteBehavior.Restrict);

        // Optimizes the "approved, newest first" query.
        builder.HasIndex(t => new { t.Status, t.CreatedAtUtc });
    }
}
