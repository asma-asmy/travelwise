using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class ApprovalDecisionConfiguration : IEntityTypeConfiguration<ApprovalDecision>
{
    public void Configure(EntityTypeBuilder<ApprovalDecision> b)
    {
        b.ToTable("ApprovalDecisions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Decision).HasConversion<int>();
        b.Property(x => x.Reason).HasMaxLength(1000);
        b.HasIndex(x => x.WorkflowRunId);
        b.HasOne(x => x.WorkflowRun).WithMany(w => w.Approvals).HasForeignKey(x => x.WorkflowRunId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.DecidedByUser).WithMany().HasForeignKey(x => x.DecidedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}