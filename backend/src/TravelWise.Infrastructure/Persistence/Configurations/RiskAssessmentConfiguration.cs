using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> b)
    {
        b.ToTable("RiskAssessments");
        b.HasKey(x => x.Id);
        b.Property(x => x.OverallLevel).HasConversion<int>();
        b.HasIndex(x => x.TripId);
        b.HasOne(x => x.Trip).WithMany(t => t.RiskAssessments).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Cascade);
    }
}