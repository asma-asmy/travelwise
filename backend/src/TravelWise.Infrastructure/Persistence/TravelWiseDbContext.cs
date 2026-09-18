using Microsoft.EntityFrameworkCore;
using TravelWise.Domain.Entities;

namespace TravelWise.Infrastructure.Persistence;

public class TravelWiseDbContext : DbContext
{
    public TravelWiseDbContext(DbContextOptions<TravelWiseDbContext> options)
        : base(options)
    {
    }

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Trips
    public DbSet<Trip> Trips => Set<Trip>();

    // Budget
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();

    // Activities
    public DbSet<TravellerPreference> TravellerPreferences => Set<TravellerPreference>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ScheduledActivity> ScheduledActivities => Set<ScheduledActivity>();

    // Risk
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();
    public DbSet<SafetyAlert> SafetyAlerts => Set<SafetyAlert>();
    public DbSet<WeatherSnapshot> WeatherSnapshots => Set<WeatherSnapshot>();

    // Documents
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentRequirement> DocumentRequirements => Set<DocumentRequirement>();
    public DbSet<ReadinessAssessment> ReadinessAssessments => Set<ReadinessAssessment>();

    // Workflow
    public DbSet<WorkflowRun> WorkflowRuns => Set<WorkflowRun>();
    public DbSet<WorkflowEvent> WorkflowEvents => Set<WorkflowEvent>();
    public DbSet<AgentExecution> AgentExecutions => Set<AgentExecution>();
    public DbSet<ToolCall> ToolCalls => Set<ToolCall>();
    public DbSet<ValidationResult> ValidationResults => Set<ValidationResult>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TravelWiseDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}