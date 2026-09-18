using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class ValidationResultConfiguration : IEntityTypeConfiguration<ValidationResult>
{
    public void Configure(EntityTypeBuilder<ValidationResult> b)
    {
        b.ToTable("ValidationResults");
        b.HasKey(x => x.Id);
        b.Property(x => x.ValidatorName).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.WorkflowRunId);
        b.HasOne(x => x.WorkflowRun).WithMany(w => w.Validations).HasForeignKey(x => x.WorkflowRunId).OnDelete(DeleteBehavior.Cascade);
    }
}