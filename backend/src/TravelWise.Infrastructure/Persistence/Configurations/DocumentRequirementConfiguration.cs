using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class DocumentRequirementConfiguration : IEntityTypeConfiguration<DocumentRequirement>
{
    public void Configure(EntityTypeBuilder<DocumentRequirement> b)
    {
        b.ToTable("DocumentRequirements");
        b.HasKey(x => x.Id);
        b.Property(x => x.Destination).IsRequired().HasMaxLength(200);
        b.Property(x => x.TripType).HasConversion<int>();
        b.Property(x => x.DocumentType).HasConversion<int>();
        b.HasIndex(x => new { x.Destination, x.TripType });
    }
}