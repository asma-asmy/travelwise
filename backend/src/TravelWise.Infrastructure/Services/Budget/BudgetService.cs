using Microsoft.EntityFrameworkCore;
using TravelWise.Application.Common.Exceptions;
using TravelWise.Application.Features.Budget;
using TravelWise.Application.Features.Budget.Dtos;
using TravelWise.Domain.Entities;
using TravelWise.Infrastructure.Persistence;
using BudgetEntity = TravelWise.Domain.Entities.Budget;

namespace TravelWise.Infrastructure.Services.Budget;

public class BudgetService : IBudgetService
{
    private readonly TravelWiseDbContext _db;

    public BudgetService(TravelWiseDbContext db)
    {
        _db = db;
    }

    // ---------- Budget ----------

    public async Task<BudgetDto?> GetBudgetAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var budget = await _db.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.TripId == tripId, ct);

        if (budget is null) return null;

        var expenses = await _db.Expenses.Where(e => e.TripId == tripId).ToListAsync(ct);

        return MapBudget(budget, expenses);
    }

    public async Task<BudgetDto> UpsertBudgetAsync(Guid tripId, Guid userId, bool isStaff, CreateOrUpdateBudgetDto dto, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var budget = await _db.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.TripId == tripId, ct);

        if (budget is null)
        {
            budget = new BudgetEntity
            {
                TripId = tripId,
                TotalAmount = dto.TotalAmount,
                Currency = dto.Currency
            };
            _db.Budgets.Add(budget);
        }
        else
        {
            budget.TotalAmount = dto.TotalAmount;
            budget.Currency = dto.Currency;
        }

        await _db.SaveChangesAsync(ct);

        var expenses = await _db.Expenses.Where(e => e.TripId == tripId).ToListAsync(ct);
        return MapBudget(budget, expenses);
    }

    // ---------- Categories ----------

    public async Task<List<BudgetCategoryDto>> GetCategoriesAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var budget = await _db.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.TripId == tripId, ct)
            ?? throw new NotFoundException("No budget has been set for this trip.");

        var expenses = await _db.Expenses.Where(e => e.TripId == tripId).ToListAsync(ct);
        return budget.Categories.Select(c => MapCategory(c, expenses)).ToList();
    }

    public async Task<BudgetCategoryDto> AddCategoryAsync(Guid tripId, Guid userId, bool isStaff, CreateOrUpdateBudgetCategoryDto dto, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var budget = await _db.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.TripId == tripId, ct)
            ?? throw new NotFoundException("Create a budget before adding categories.");

        // Business rule: total category limits must not exceed the budget total
        var currentSum = budget.Categories.Sum(c => c.LimitAmount);
        if (currentSum + dto.LimitAmount > budget.TotalAmount)
            throw new AppException(
                $"Category limits would exceed the total budget of {budget.TotalAmount:N2} {budget.Currency}.",
                422);

        var category = new BudgetCategory
        {
            BudgetId = budget.Id,
            Name = dto.Name.Trim(),
            LimitAmount = dto.LimitAmount
        };
        _db.BudgetCategories.Add(category);
        await _db.SaveChangesAsync(ct);

        var expenses = await _db.Expenses.Where(e => e.TripId == tripId).ToListAsync(ct);
        return MapCategory(category, expenses);
    }

    public async Task<BudgetCategoryDto> UpdateCategoryAsync(Guid tripId, Guid categoryId, Guid userId, bool isStaff, CreateOrUpdateBudgetCategoryDto dto, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var budget = await _db.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.TripId == tripId, ct)
            ?? throw new NotFoundException("Budget not found.");

        var category = budget.Categories.FirstOrDefault(c => c.Id == categoryId)
            ?? throw new NotFoundException("Category not found.");

        // Business rule: total category limits must not exceed the budget total (excluding current)
        var currentSum = budget.Categories.Where(c => c.Id != categoryId).Sum(c => c.LimitAmount);
        if (currentSum + dto.LimitAmount > budget.TotalAmount)
            throw new AppException(
                $"Category limits would exceed the total budget of {budget.TotalAmount:N2} {budget.Currency}.",
                422);

        category.Name = dto.Name.Trim();
        category.LimitAmount = dto.LimitAmount;
        await _db.SaveChangesAsync(ct);

        var expenses = await _db.Expenses.Where(e => e.TripId == tripId).ToListAsync(ct);
        return MapCategory(category, expenses);
    }

    public async Task DeleteCategoryAsync(Guid tripId, Guid categoryId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var budget = await _db.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.TripId == tripId, ct)
            ?? throw new NotFoundException("Budget not found.");

        var category = budget.Categories.FirstOrDefault(c => c.Id == categoryId)
            ?? throw new NotFoundException("Category not found.");

        _db.BudgetCategories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }

    // ---------- Expenses ----------

    public async Task<PagedResult<ExpenseDto>> ListExpensesAsync(Guid tripId, Guid userId, bool isStaff, ExpenseQueryDto query, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var q = _db.Expenses
            .Include(e => e.BudgetCategory)
            .Where(e => e.TripId == tripId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(e => EF.Functions.ILike(e.Description, $"%{query.Search}%"));

        if (query.BudgetCategoryId.HasValue)
            q = q.Where(e => e.BudgetCategoryId == query.BudgetCategoryId);

        if (query.FromDate.HasValue)
            q = q.Where(e => e.SpentAt >= query.FromDate);

        if (query.ToDate.HasValue)
            q = q.Where(e => e.SpentAt <= query.ToDate);

        if (query.MinAmount.HasValue)
            q = q.Where(e => e.Amount >= query.MinAmount);

        if (query.MaxAmount.HasValue)
            q = q.Where(e => e.Amount <= query.MaxAmount);

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "amount" => query.SortDir == "asc" ? q.OrderBy(e => e.Amount) : q.OrderByDescending(e => e.Amount),
            _ => query.SortDir == "asc" ? q.OrderBy(e => e.SpentAt) : q.OrderByDescending(e => e.SpentAt)
        };

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapExpenseExpr)
            .ToListAsync(ct);

        return new PagedResult<ExpenseDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ExpenseDto> AddExpenseAsync(Guid tripId, Guid userId, bool isStaff, CreateExpenseDto dto, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        if (dto.BudgetCategoryId.HasValue)
        {
            var belongs = await _db.BudgetCategories
                .Join(_db.Budgets, c => c.BudgetId, b => b.Id, (c, b) => new { c.Id, b.TripId })
                .AnyAsync(x => x.Id == dto.BudgetCategoryId && x.TripId == tripId, ct);

            if (!belongs)
                throw new AppException("Budget category does not belong to this trip.", 422);
        }

        var expense = new Expense
        {
            TripId = tripId,
            BudgetCategoryId = dto.BudgetCategoryId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Description = dto.Description.Trim(),
            SpentAt = dto.SpentAt ?? DateTime.UtcNow
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(expense).Reference(e => e.BudgetCategory).LoadAsync(ct);
        return MapExpense(expense);
    }

    public async Task<ExpenseDto> UpdateExpenseAsync(Guid tripId, Guid expenseId, Guid userId, bool isStaff, UpdateExpenseDto dto, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var expense = await _db.Expenses
            .Include(e => e.BudgetCategory)
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.TripId == tripId, ct)
            ?? throw new NotFoundException("Expense not found.");

        if (dto.BudgetCategoryId.HasValue)
        {
            var belongs = await _db.BudgetCategories
                .Join(_db.Budgets, c => c.BudgetId, b => b.Id, (c, b) => new { c.Id, b.TripId })
                .AnyAsync(x => x.Id == dto.BudgetCategoryId && x.TripId == tripId, ct);

            if (!belongs)
                throw new AppException("Budget category does not belong to this trip.", 422);
        }

        expense.BudgetCategoryId = dto.BudgetCategoryId;
        expense.Amount = dto.Amount;
        expense.Currency = dto.Currency;
        expense.Description = dto.Description.Trim();
        expense.SpentAt = dto.SpentAt;

        await _db.SaveChangesAsync(ct);
        await _db.Entry(expense).Reference(e => e.BudgetCategory).LoadAsync(ct);
        return MapExpense(expense);
    }

    public async Task DeleteExpenseAsync(Guid tripId, Guid expenseId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId && e.TripId == tripId, ct)
            ?? throw new NotFoundException("Expense not found.");

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync(ct);
    }

    // ---------- Business Operation: Health ----------

    public async Task<BudgetHealthDto> GetHealthAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        var trip = await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var budget = await _db.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.TripId == tripId, ct);

        var expenses = await _db.Expenses.Where(e => e.TripId == tripId).ToListAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return BudgetHealthCalculator.Calculate(trip, budget, expenses, today);
    }

    // ---------- Helpers ----------

    private async Task<Trip> EnsureTripAccessAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct)
            ?? throw new NotFoundException("Trip not found.");

        if (!isStaff && trip.UserId != userId)
            throw new ForbiddenException("You do not have access to this trip.");

        return trip;
    }

    private static BudgetDto MapBudget(BudgetEntity budget, IReadOnlyCollection<Expense> expenses)
    {
        var totalSpent = expenses.Sum(e => e.Amount);
        return new BudgetDto
        {
            Id = budget.Id,
            TripId = budget.TripId,
            TotalAmount = budget.TotalAmount,
            Currency = budget.Currency,
            TotalSpent = totalSpent,
            RemainingAmount = budget.TotalAmount - totalSpent,
            Categories = budget.Categories.Select(c => MapCategory(c, expenses)).ToList(),
            CreatedAt = budget.CreatedAt,
            UpdatedAt = budget.UpdatedAt
        };
    }

    private static BudgetCategoryDto MapCategory(BudgetCategory category, IReadOnlyCollection<Expense> expenses)
    {
        var spent = expenses.Where(e => e.BudgetCategoryId == category.Id).Sum(e => e.Amount);
        return new BudgetCategoryDto
        {
            Id = category.Id,
            BudgetId = category.BudgetId,
            Name = category.Name,
            LimitAmount = category.LimitAmount,
            SpentAmount = spent,
            RemainingAmount = category.LimitAmount - spent,
            IsOverLimit = spent > category.LimitAmount
        };
    }

    private static ExpenseDto MapExpense(Expense e) => new()
    {
        Id = e.Id,
        TripId = e.TripId,
        BudgetCategoryId = e.BudgetCategoryId,
        BudgetCategoryName = e.BudgetCategory?.Name,
        Amount = e.Amount,
        Currency = e.Currency,
        Description = e.Description,
        SpentAt = e.SpentAt,
        CreatedAt = e.CreatedAt
    };

    private static System.Linq.Expressions.Expression<Func<Expense, ExpenseDto>> MapExpenseExpr => e => new ExpenseDto
    {
        Id = e.Id,
        TripId = e.TripId,
        BudgetCategoryId = e.BudgetCategoryId,
        BudgetCategoryName = e.BudgetCategory != null ? e.BudgetCategory.Name : null,
        Amount = e.Amount,
        Currency = e.Currency,
        Description = e.Description,
        SpentAt = e.SpentAt,
        CreatedAt = e.CreatedAt
    };
}