using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.ToTable("Documents");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<int>();
        b.Property(x => x.Number).HasMaxLength(128);
        b.Property(x => x.FileRef).HasMaxLength(500);
        b.HasIndex(x => new { x.UserId, x.ExpiryDate });
        b.HasOne(x => x.User).WithMany(u => u.Documents).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}