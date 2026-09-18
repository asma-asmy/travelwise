using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class BudgetCategoryConfiguration : IEntityTypeConfiguration<BudgetCategory>
{
    public void Configure(EntityTypeBuilder<BudgetCategory> b)
    {
        b.ToTable("BudgetCategories");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.LimitAmount).HasPrecision(18, 2);
        b.HasIndex(x => x.BudgetId);
        b.HasOne(x => x.Budget).WithMany(x => x.Categories).HasForeignKey(x => x.BudgetId).OnDelete(DeleteBehavior.Cascade);
    }
}