using Microsoft.EntityFrameworkCore;

namespace TravelWise.Infrastructure.Persistence;

public class TravelWiseDbContext : DbContext
{
    public TravelWiseDbContext(DbContextOptions<TravelWiseDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TravelWiseDbContext).Assembly);
    }
}