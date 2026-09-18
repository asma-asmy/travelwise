using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class SafetyAlertConfiguration : IEntityTypeConfiguration<SafetyAlert>
{
    public void Configure(EntityTypeBuilder<SafetyAlert> b)
    {
        b.ToTable("SafetyAlerts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Destination).IsRequired().HasMaxLength(200);
        b.Property(x => x.Category).IsRequired().HasMaxLength(64);
        b.Property(x => x.Severity).HasConversion<int>();
        b.Property(x => x.Message).IsRequired().HasMaxLength(500);
        b.HasIndex(x => new { x.Destination, x.IsActive });
    }
}