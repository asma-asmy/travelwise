using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> b)
    {
        b.ToTable("Expenses", t => t.HasCheckConstraint("CK_Expense_Amount_Positive", "\"Amount\" > 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Currency).IsRequired().HasMaxLength(8);
        b.Property(x => x.Description).IsRequired().HasMaxLength(300);
        b.HasIndex(x => new { x.TripId, x.SpentAt });
        b.HasOne(x => x.Trip).WithMany(t => t.Expenses).HasForeignKey(x => x.TripId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.BudgetCategory).WithMany(c => c.Expenses).HasForeignKey(x => x.BudgetCategoryId).OnDelete(DeleteBehavior.SetNull);
    }
}