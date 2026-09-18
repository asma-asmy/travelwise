using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class ScheduledActivityConfiguration : IEntityTypeConfiguration<ScheduledActivity>
{
    public void Configure(EntityTypeBuilder<ScheduledActivity> b)
    {
        b.ToTable("ScheduledActivities");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasMaxLength(32);
        b.HasIndex(x => new { x.TripId, x.ScheduledDate });
        b.HasOne(x => x.Trip).WithMany(t => t.ScheduledActivities).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Activity).WithMany(a => a.ScheduledActivities).HasForeignKey(x => x.ActivityId).OnDelete(DeleteBehavior.Restrict);
    }
}