namespace TravelWise.Application.Features.Readiness.Dtos;

public class ReadinessAssessmentDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public int Score { get; set; }                            // 0..100
    public DateTime AssessedAt { get; set; }
    public List<MissingDocumentDto> MissingDocuments { get; set; } = new();
    public List<ExpiringDocumentDto> ExpiringDocuments { get; set; } = new();
    public List<string> BlockingIssues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class MissingDocumentDto
{
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public string? Notes { get; set; }
}

public class ExpiringDocumentDto
{
    public Guid DocumentId { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool ExpiresDuringTrip { get; set; }
}