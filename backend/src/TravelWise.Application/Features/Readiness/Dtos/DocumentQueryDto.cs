using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Readiness.Dtos;

public class DocumentQueryDto
{
    public DocumentType? Type { get; set; }
    public bool? ExpiredOnly { get; set; }
    public bool? ExpiringWithinDays { get; set; }           // if true, filter to docs expiring in next 90 days
    public string SortBy { get; set; } = "ExpiryDate";     // ExpiryDate | Type
    public string SortDir { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}