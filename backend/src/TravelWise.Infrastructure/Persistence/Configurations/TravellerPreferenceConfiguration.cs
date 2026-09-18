using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class TravellerPreferenceConfiguration : IEntityTypeConfiguration<TravellerPreference>
{
    public void Configure(EntityTypeBuilder<TravellerPreference> b)
    {
        b.ToTable("TravellerPreferences");
        b.HasKey(x => x.Id);
        b.Property(x => x.Pace).HasMaxLength(32);
        b.Property(x => x.BudgetTier).HasMaxLength(32);
        b.HasIndex(x => x.UserId).IsUnique();
        b.HasOne(x => x.User).WithOne(u => u.Preference).HasForeignKey<TravellerPreference>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}