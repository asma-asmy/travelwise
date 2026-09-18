using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class WorkflowRunConfiguration : IEntityTypeConfiguration<WorkflowRun>
{
    public void Configure(EntityTypeBuilder<WorkflowRun> b)
    {
        b.ToTable("WorkflowRuns");
        b.HasKey(x => x.Id);
        b.Property(x => x.Objective).IsRequired().HasMaxLength(1000);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.CurrentStep).HasMaxLength(100);
        b.HasIndex(x => new { x.TripId, x.Status });
        b.HasIndex(x => x.Status);
        b.HasOne(x => x.Trip).WithMany(t => t.WorkflowRuns).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Cascade);
    }
}