namespace TravelWise.Application.Features.Budget.Dtos;

public class ExpenseQueryDto
{
    public string? Search { get; set; }                        // matches description
    public Guid? BudgetCategoryId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string SortBy { get; set; } = "SpentAt";            // SpentAt | Amount
    public string SortDir { get; set; } = "desc";              // asc | desc
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}