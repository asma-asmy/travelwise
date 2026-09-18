using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class WorkflowEventConfiguration : IEntityTypeConfiguration<WorkflowEvent>
{
    public void Configure(EntityTypeBuilder<WorkflowEvent> b)
    {
        b.ToTable("WorkflowEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.WorkflowRunId);
        b.HasOne(x => x.WorkflowRun).WithMany(w => w.Events).HasForeignKey(x => x.WorkflowRunId).OnDelete(DeleteBehavior.Cascade);
    }
}