using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class AgentExecutionConfiguration : IEntityTypeConfiguration<AgentExecution>
{
    public void Configure(EntityTypeBuilder<AgentExecution> b)
    {
        b.ToTable("AgentExecutions");
        b.HasKey(x => x.Id);
        b.Property(x => x.AgentName).IsRequired().HasMaxLength(100);
        b.Property(x => x.TaskName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => x.WorkflowRunId);
        b.HasOne(x => x.WorkflowRun).WithMany(w => w.Executions).HasForeignKey(x => x.WorkflowRunId).OnDelete(DeleteBehavior.Cascade);
    }
}