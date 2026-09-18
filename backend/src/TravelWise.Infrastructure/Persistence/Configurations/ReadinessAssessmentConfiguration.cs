using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class ReadinessAssessmentConfiguration : IEntityTypeConfiguration<ReadinessAssessment>
{
    public void Configure(EntityTypeBuilder<ReadinessAssessment> b)
    {
        b.ToTable("ReadinessAssessments");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.TripId);
        b.HasOne(x => x.Trip).WithMany(t => t.ReadinessAssessments).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Cascade);
    }
}