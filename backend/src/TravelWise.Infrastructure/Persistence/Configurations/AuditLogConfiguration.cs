using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(x => x.Id);
        b.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
        b.Property(x => x.EntityId).IsRequired().HasMaxLength(100);
        b.Property(x => x.Action).IsRequired().HasMaxLength(100);
        b.HasIndex(x => new { x.EntityType, x.EntityId });
        b.HasIndex(x => x.CreatedAt);
    }
}