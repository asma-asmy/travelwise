using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> b)
    {
        b.ToTable("Activities");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Category).HasConversion<int>();
        b.Property(x => x.EstimatedCost).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(8);
        b.Property(x => x.Location).HasMaxLength(200);
        b.Property(x => x.Difficulty).HasMaxLength(32);
        b.HasIndex(x => x.Category);
        b.HasIndex(x => x.IsActive);
    }
}