using TravelWise.Application.Features.Budget.Dtos;

namespace TravelWise.Application.Features.Budget;

public interface IBudgetService
{
    Task<BudgetDto?> GetBudgetAsync(Guid tripId, Guid requestingUserId, bool isStaff, CancellationToken ct = default);
    Task<BudgetDto> UpsertBudgetAsync(Guid tripId, Guid requestingUserId, bool isStaff, CreateOrUpdateBudgetDto dto, CancellationToken ct = default);

    Task<List<BudgetCategoryDto>> GetCategoriesAsync(Guid tripId, Guid requestingUserId, bool isStaff, CancellationToken ct = default);
    Task<BudgetCategoryDto> AddCategoryAsync(Guid tripId, Guid requestingUserId, bool isStaff, CreateOrUpdateBudgetCategoryDto dto, CancellationToken ct = default);
    Task<BudgetCategoryDto> UpdateCategoryAsync(Guid tripId, Guid categoryId, Guid requestingUserId, bool isStaff, CreateOrUpdateBudgetCategoryDto dto, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid tripId, Guid categoryId, Guid requestingUserId, bool isStaff, CancellationToken ct = default);

    Task<PagedResult<ExpenseDto>> ListExpensesAsync(Guid tripId, Guid requestingUserId, bool isStaff, ExpenseQueryDto query, CancellationToken ct = default);
    Task<ExpenseDto> AddExpenseAsync(Guid tripId, Guid requestingUserId, bool isStaff, CreateExpenseDto dto, CancellationToken ct = default);
    Task<ExpenseDto> UpdateExpenseAsync(Guid tripId, Guid expenseId, Guid requestingUserId, bool isStaff, UpdateExpenseDto dto, CancellationToken ct = default);
    Task DeleteExpenseAsync(Guid tripId, Guid expenseId, Guid requestingUserId, bool isStaff, CancellationToken ct = default);

    Task<BudgetHealthDto> GetHealthAsync(Guid tripId, Guid requestingUserId, bool isStaff, CancellationToken ct = default);
}