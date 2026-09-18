using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> b)
    {
        b.ToTable("Budgets", t => t.HasCheckConstraint("CK_Budget_Total_Positive", "\"TotalAmount\" > 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.Currency).IsRequired().HasMaxLength(8);
        b.HasIndex(x => x.TripId).IsUnique();
        b.HasOne(x => x.Trip).WithOne(t => t.Budget).HasForeignKey<Budget>(x => x.TripId).OnDelete(DeleteBehavior.Cascade);
    }
}