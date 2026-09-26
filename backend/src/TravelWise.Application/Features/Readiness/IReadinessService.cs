using TravelWise.Application.Features.Budget.Dtos;   // PagedResult<T>
using TravelWise.Application.Features.Readiness.Dtos;

namespace TravelWise.Application.Features.Readiness;

public interface IReadinessService
{
    // Documents (owner-scoped)
    Task<PagedResult<DocumentDto>> ListMyDocumentsAsync(Guid userId, DocumentQueryDto query, CancellationToken ct = default);
    Task<DocumentDto> GetMyDocumentAsync(Guid userId, Guid documentId, CancellationToken ct = default);
    Task<DocumentDto> AddMyDocumentAsync(Guid userId, CreateDocumentDto dto, CancellationToken ct = default);
    Task<DocumentDto> UpdateMyDocumentAsync(Guid userId, Guid documentId, UpdateDocumentDto dto, CancellationToken ct = default);
    Task DeleteMyDocumentAsync(Guid userId, Guid documentId, CancellationToken ct = default);

    // Requirements (admin)
    Task<List<DocumentRequirementDto>> ListRequirementsAsync(string? destination, CancellationToken ct = default);
    Task<DocumentRequirementDto> CreateRequirementAsync(CreateDocumentRequirementDto dto, CancellationToken ct = default);
    Task<DocumentRequirementDto> UpdateRequirementAsync(Guid id, UpdateDocumentRequirementDto dto, CancellationToken ct = default);
    Task DeleteRequirementAsync(Guid id, CancellationToken ct = default);

    // Business operations
    Task<ReadinessAssessmentDto> AssessTripAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default);
    Task<ReadinessAssessmentDto?> GetLatestAssessmentAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default);
}