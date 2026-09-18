using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class ToolCallConfiguration : IEntityTypeConfiguration<ToolCall>
{
    public void Configure(EntityTypeBuilder<ToolCall> b)
    {
        b.ToTable("ToolCalls");
        b.HasKey(x => x.Id);
        b.Property(x => x.ToolName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => x.AgentExecutionId);
        b.HasOne(x => x.AgentExecution).WithMany(a => a.ToolCalls).HasForeignKey(x => x.AgentExecutionId).OnDelete(DeleteBehavior.Cascade);
    }
}