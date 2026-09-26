using Microsoft.EntityFrameworkCore;
using TravelWise.Application.Common.Exceptions;
using TravelWise.Application.Features.Budget.Dtos;
using TravelWise.Application.Features.Readiness;
using TravelWise.Application.Features.Readiness.Dtos;
using TravelWise.Infrastructure.Persistence;
using DocumentEntity = TravelWise.Domain.Entities.Document;
using DocumentRequirementEntity = TravelWise.Domain.Entities.DocumentRequirement;
using ReadinessAssessmentEntity = TravelWise.Domain.Entities.ReadinessAssessment;
using TripEntity = TravelWise.Domain.Entities.Trip;

namespace TravelWise.Infrastructure.Services.Readiness;

public class ReadinessService : IReadinessService
{
    private readonly TravelWiseDbContext _db;

    public ReadinessService(TravelWiseDbContext db)
    {
        _db = db;
    }

    // ---------------- Documents ----------------

    public async Task<PagedResult<DocumentDto>> ListMyDocumentsAsync(Guid userId, DocumentQueryDto query, CancellationToken ct = default)
    {
        var q = _db.Documents.Where(d => d.UserId == userId).AsQueryable();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (query.Type.HasValue)
            q = q.Where(d => d.Type == query.Type);

        if (query.ExpiredOnly == true)
            q = q.Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < today);

        if (query.ExpiringWithinDays == true)
        {
            var cutoff = today.AddDays(90);
            q = q.Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value >= today && d.ExpiryDate.Value <= cutoff);
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "type" => query.SortDir == "desc" ? q.OrderByDescending(d => d.Type) : q.OrderBy(d => d.Type),
            _ => query.SortDir == "desc" ? q.OrderByDescending(d => d.ExpiryDate) : q.OrderBy(d => d.ExpiryDate)
        };

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<DocumentDto>
        {
            Items = items.Select(d => MapDocument(d, today)).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DocumentDto> GetMyDocumentAsync(Guid userId, Guid documentId, CancellationToken ct = default)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId, ct)
            ?? throw new NotFoundException("Document not found.");
        return MapDocument(doc, DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task<DocumentDto> AddMyDocumentAsync(Guid userId, CreateDocumentDto dto, CancellationToken ct = default)
    {
        var doc = new DocumentEntity
        {
            UserId = userId,
            Type = dto.Type,
            Number = dto.Number.Trim(),
            IssuedDate = dto.IssuedDate,
            ExpiryDate = dto.ExpiryDate,
            FileRef = dto.FileRef,
            Notes = dto.Notes
        };
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync(ct);
        return MapDocument(doc, DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task<DocumentDto> UpdateMyDocumentAsync(Guid userId, Guid documentId, UpdateDocumentDto dto, CancellationToken ct = default)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId, ct)
            ?? throw new NotFoundException("Document not found.");

        doc.Type = dto.Type;
        doc.Number = dto.Number.Trim();
        doc.IssuedDate = dto.IssuedDate;
        doc.ExpiryDate = dto.ExpiryDate;
        doc.FileRef = dto.FileRef;
        doc.Notes = dto.Notes;

        await _db.SaveChangesAsync(ct);
        return MapDocument(doc, DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task DeleteMyDocumentAsync(Guid userId, Guid documentId, CancellationToken ct = default)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId, ct)
            ?? throw new NotFoundException("Document not found.");

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Requirements ----------------

    public async Task<List<DocumentRequirementDto>> ListRequirementsAsync(string? destination, CancellationToken ct = default)
    {
        var q = _db.DocumentRequirements.AsQueryable();
        if (!string.IsNullOrWhiteSpace(destination))
            q = q.Where(r => EF.Functions.ILike(r.Destination, $"%{destination}%"));

        var list = await q.OrderBy(r => r.Destination).ThenBy(r => r.DocumentType).ToListAsync(ct);
        return list.Select(MapRequirement).ToList();
    }

    public async Task<DocumentRequirementDto> CreateRequirementAsync(CreateDocumentRequirementDto dto, CancellationToken ct = default)
    {
        // Uniqueness: same destination + tripType + documentType should not exist
        var exists = await _db.DocumentRequirements.AnyAsync(
            r => r.Destination == dto.Destination.Trim() &&
                 r.TripType == dto.TripType &&
                 r.DocumentType == dto.DocumentType,
            ct);

        if (exists)
            throw new ConflictException(
                $"A requirement for {dto.DocumentType} on {dto.Destination} ({dto.TripType}) already exists.");

        var entity = new DocumentRequirementEntity
        {
            Destination = dto.Destination.Trim(),
            TripType = dto.TripType,
            DocumentType = dto.DocumentType,
            IsMandatory = dto.IsMandatory,
            Notes = dto.Notes
        };
        _db.DocumentRequirements.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapRequirement(entity);
    }

    public async Task<DocumentRequirementDto> UpdateRequirementAsync(Guid id, UpdateDocumentRequirementDto dto, CancellationToken ct = default)
    {
        var entity = await _db.DocumentRequirements.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("Requirement not found.");

        entity.TripType = dto.TripType;
        entity.DocumentType = dto.DocumentType;
        entity.IsMandatory = dto.IsMandatory;
        entity.Notes = dto.Notes;

        await _db.SaveChangesAsync(ct);
        return MapRequirement(entity);
    }

    public async Task DeleteRequirementAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.DocumentRequirements.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("Requirement not found.");
        _db.DocumentRequirements.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Business operations ----------------

    public async Task<ReadinessAssessmentDto> AssessTripAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        var trip = await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var held = await _db.Documents.Where(d => d.UserId == trip.UserId).ToListAsync(ct);
        var requirements = await _db.DocumentRequirements
            .Where(r => r.Destination == trip.Destination && r.TripType == trip.TripType)
            .ToListAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = ReadinessCalculator.Calculate(trip, held, requirements, today);

        // Persist
        var entity = new ReadinessAssessmentEntity
        {
            TripId = trip.Id,
            Score = result.Score,
            MissingJson = System.Text.Json.JsonSerializer.Serialize(result.MissingDocuments),
            ExpiringJson = System.Text.Json.JsonSerializer.Serialize(result.ExpiringDocuments),
            BlockingJson = System.Text.Json.JsonSerializer.Serialize(result.BlockingIssues),
            AssessedAt = DateTime.UtcNow
        };
        _db.ReadinessAssessments.Add(entity);
        await _db.SaveChangesAsync(ct);

        result.Id = entity.Id;
        return result;
    }

    public async Task<ReadinessAssessmentDto?> GetLatestAssessmentAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        var latest = await _db.ReadinessAssessments
            .Where(r => r.TripId == tripId)
            .OrderByDescending(r => r.AssessedAt)
            .FirstOrDefaultAsync(ct);

        if (latest is null) return null;

        try
        {
            var missing = System.Text.Json.JsonSerializer.Deserialize<List<MissingDocumentDto>>(latest.MissingJson) ?? new();
            var expiring = System.Text.Json.JsonSerializer.Deserialize<List<ExpiringDocumentDto>>(latest.ExpiringJson) ?? new();
            var blocking = System.Text.Json.JsonSerializer.Deserialize<List<string>>(latest.BlockingJson) ?? new();

            return new ReadinessAssessmentDto
            {
                Id = latest.Id,
                TripId = latest.TripId,
                Score = latest.Score,
                AssessedAt = latest.AssessedAt,
                MissingDocuments = missing,
                ExpiringDocuments = expiring,
                BlockingIssues = blocking
            };
        }
        catch
        {
            return null;
        }
    }

    // ---------------- Helpers ----------------

    private async Task<TripEntity> EnsureTripAccessAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct)
            ?? throw new NotFoundException("Trip not found.");

        if (!isStaff && trip.UserId != userId)
            throw new ForbiddenException("You do not have access to this trip.");

        return trip;
    }

    private static DocumentDto MapDocument(DocumentEntity d, DateOnly today)
    {
        int? daysLeft = d.ExpiryDate.HasValue ? d.ExpiryDate.Value.DayNumber - today.DayNumber : null;
        return new DocumentDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Type = d.Type,
            TypeName = d.Type.ToString(),
            Number = d.Number,
            IssuedDate = d.IssuedDate,
            ExpiryDate = d.ExpiryDate,
            FileRef = d.FileRef,
            Notes = d.Notes,
            IsExpired = d.ExpiryDate.HasValue && d.ExpiryDate.Value < today,
            DaysUntilExpiry = daysLeft,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
    }

    private static DocumentRequirementDto MapRequirement(DocumentRequirementEntity r) => new()
    {
        Id = r.Id,
        Destination = r.Destination,
        TripType = r.TripType,
        TripTypeName = r.TripType.ToString(),
        DocumentType = r.DocumentType,
        DocumentTypeName = r.DocumentType.ToString(),
        IsMandatory = r.IsMandatory,
        Notes = r.Notes
    };
}