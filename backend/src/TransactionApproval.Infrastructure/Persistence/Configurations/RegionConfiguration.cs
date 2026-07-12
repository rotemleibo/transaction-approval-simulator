using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Infrastructure.Persistence.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Regions");

        builder.HasKey(r => r.Code);

        builder.Property(r => r.Code).HasMaxLength(16);
        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.Property(r => r.TimeZoneId).HasMaxLength(64).IsRequired();

        builder.HasData(RegionCatalog.Regions.Select(r => new Region
        {
            Code = r.Code,
            Name = r.Name,
            TimeZoneId = r.TimeZoneId
        }));
    }
}
