using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelWise.Application.Features.Budget;
using TravelWise.Application.Features.Budget.Dtos;

namespace TravelWise.Api.Controllers;

[ApiController]
[Route("api/v1/trips/{tripId:guid}/budget")]
[Authorize]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _budget;

    public BudgetController(IBudgetService budget)
    {
        _budget = budget;
    }

    // ---------- Budget ----------

    [HttpGet]
    public async Task<ActionResult<BudgetDto?>> GetBudget(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _budget.GetBudgetAsync(tripId, userId, isStaff, ct);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<BudgetDto>> UpsertBudget(Guid tripId, [FromBody] CreateOrUpdateBudgetDto dto, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _budget.UpsertBudgetAsync(tripId, userId, isStaff, dto, ct);
        return Ok(result);
    }

    // ---------- Categories ----------

    [HttpGet("categories")]
    public async Task<ActionResult<List<BudgetCategoryDto>>> GetCategories(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _budget.GetCategoriesAsync(tripId, userId, isStaff, ct);
        return Ok(result);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<BudgetCategoryDto>> AddCategory(Guid tripId, [FromBody] CreateOrUpdateBudgetCategoryDto dto, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _budget.AddCategoryAsync(tripId, userId, isStaff, dto, ct);
        return CreatedAtAction(nameof(GetCategories), new { tripId }, result);
    }

    [HttpPut("categories/{categoryId:guid}")]
    public async Task<ActionResult<BudgetCategoryDto>> UpdateCategory(Guid tripId, Guid categoryId, [FromBody] CreateOrUpdateBudgetCategoryDto dto, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _budget.UpdateCategoryAsync(tripId, categoryId, userId, isStaff, dto, ct);
        return Ok(result);
    }

    [HttpDelete("categories/{categoryId:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid tripId, Guid categoryId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        await _budget.DeleteCategoryAsync(tripId, categoryId, userId, isStaff, ct);
        return NoContent();
    }

    // ---------- Expenses ----------

    [HttpGet("expenses")]
    public async Task<ActionResult<PagedResult<ExpenseDto>>> ListExpenses(Guid tripId, [FromQuery] ExpenseQueryDto query, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _budget.ListExpensesAsync(tripId, userId, isStaff, query, ct);
        return Ok(result);
    }

    [HttpPost("expenses")]
    public async Task<ActionResult<ExpenseDto>> AddExpense(Guid tripId, [FromBody] CreateExpenseDto dto, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _budget.AddExpenseAsync(tripId, userId, isStaff, dto, ct);
        return CreatedAtAction(nameof(ListExpenses), new { tripId }, result);
    }

    [HttpPut("expenses/{expenseId:guid}")]
    public async Task<ActionResult<ExpenseDto>> UpdateExpense(Guid tripId, Guid expenseId, [FromBody] UpdateExpenseDto dto, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _budget.UpdateExpenseAsync(tripId, expenseId, userId, isStaff, dto, ct);
        return Ok(result);
    }

    [HttpDelete("expenses/{expenseId:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid tripId, Guid expenseId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        await _budget.DeleteExpenseAsync(tripId, expenseId, userId, isStaff, ct);
        return NoContent();
    }

    // ---------- Business Operation ----------

    [HttpGet("health")]
    public async Task<ActionResult<BudgetHealthDto>> GetHealth(Guid tripId, CancellationToken ct)
    {
        var (userId, isStaff) = GetUserContext();
        var result = await _budget.GetHealthAsync(tripId, userId, isStaff, ct);
        return Ok(result);
    }

    // ---------- Helpers ----------

    private (Guid userId, bool isStaff) GetUserContext()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user claim.");

        var isStaff = User.IsInRole("TravelOpsStaff") || User.IsInRole("Admin");
        return (userId, isStaff);
    }
}