using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> b)
    {
        b.ToTable("Trips");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Destination).IsRequired().HasMaxLength(200);
        b.Property(x => x.Origin).HasMaxLength(200);
        b.Property(x => x.Currency).IsRequired().HasMaxLength(8);
        b.Property(x => x.TripType).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => new { x.UserId, x.Status });
        b.HasOne(x => x.User).WithMany(u => u.Trips).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}