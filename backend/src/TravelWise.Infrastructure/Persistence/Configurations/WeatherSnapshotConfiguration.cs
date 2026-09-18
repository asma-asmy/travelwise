using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class WeatherSnapshotConfiguration : IEntityTypeConfiguration<WeatherSnapshot>
{
    public void Configure(EntityTypeBuilder<WeatherSnapshot> b)
    {
        b.ToTable("WeatherSnapshots");
        b.HasKey(x => x.Id);
        b.Property(x => x.Destination).IsRequired().HasMaxLength(200);
        b.HasIndex(x => new { x.Destination, x.Date }).IsUnique();
        b.HasIndex(x => x.ExpiresAt);
    }
}