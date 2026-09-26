using TravelWise.Application.Features.Readiness.Dtos;
using DocumentEntity = TravelWise.Domain.Entities.Document;
using DocumentRequirementEntity = TravelWise.Domain.Entities.DocumentRequirement;
using TripEntity = TravelWise.Domain.Entities.Trip;

namespace TravelWise.Application.Features.Readiness;

/// <summary>
/// Deterministic travel readiness score (0..100).
/// Combines: mandatory requirements met, optional requirements met, expiry safety.
/// Same inputs -> same output. No AI.
/// </summary>
public static class ReadinessCalculator
{
    private const int ExpiryWarningDays = 90;    // expiring soon threshold
    private const int ExpiryCriticalDays = 30;   // urgent threshold

    public static ReadinessAssessmentDto Calculate(
        TripEntity trip,
        IReadOnlyCollection<DocumentEntity> heldDocuments,
        IReadOnlyCollection<DocumentRequirementEntity> requirements,
        DateOnly today)
    {
        var missing = new List<MissingDocumentDto>();
        var expiring = new List<ExpiringDocumentDto>();
        var blocking = new List<string>();
        var warnings = new List<string>();

        var mandatory = requirements.Where(r => r.IsMandatory).ToList();
        var optional = requirements.Where(r => !r.IsMandatory).ToList();

        // Match held documents by type
        var heldByType = heldDocuments
            .GroupBy(d => d.Type)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.ExpiryDate).ToList());

        int mandatoryMet = 0;
        int optionalMet = 0;

        // ---- Mandatory requirements ----
        foreach (var req in mandatory)
        {
            if (!heldByType.TryGetValue(req.DocumentType, out var docs) || docs.Count == 0)
            {
                missing.Add(new MissingDocumentDto
                {
                    DocumentTypeName = req.DocumentType.ToString(),
                    IsMandatory = true,
                    Notes = req.Notes
                });
                blocking.Add($"Missing mandatory document: {req.DocumentType}.");
                continue;
            }

            var best = docs.First();
            if (IsExpired(best, today))
            {
                blocking.Add($"Mandatory document expired: {req.DocumentType} (expired {best.ExpiryDate:yyyy-MM-dd}).");
                continue;
            }

            if (ExpiresDuringTrip(best, trip))
            {
                blocking.Add($"Mandatory document expires during trip: {req.DocumentType}.");
                continue;
            }

            mandatoryMet++;

            // Expiry warning for mandatory docs approaching expiry
            if (best.ExpiryDate.HasValue)
            {
                var daysLeft = best.ExpiryDate.Value.DayNumber - today.DayNumber;
                if (daysLeft <= ExpiryWarningDays)
                {
                    expiring.Add(new ExpiringDocumentDto
                    {
                        DocumentId = best.Id,
                        DocumentTypeName = req.DocumentType.ToString(),
                        ExpiryDate = best.ExpiryDate.Value,
                        DaysUntilExpiry = daysLeft,
                        ExpiresDuringTrip = false
                    });
                    if (daysLeft <= ExpiryCriticalDays)
                        warnings.Add($"{req.DocumentType} expires in {daysLeft} days.");
                }
            }
        }

        // ---- Optional requirements ----
        foreach (var req in optional)
        {
            if (heldByType.TryGetValue(req.DocumentType, out var docs) && docs.Count > 0)
            {
                var best = docs.First();
                if (!IsExpired(best, today) && !ExpiresDuringTrip(best, trip))
                    optionalMet++;
            }
        }

        // ---- Also warn about held documents expiring during the trip (not tied to a requirement) ----
        foreach (var doc in heldDocuments)
        {
            if (doc.ExpiryDate.HasValue &&
                doc.ExpiryDate.Value >= trip.StartDate &&
                doc.ExpiryDate.Value <= trip.EndDate)
            {
                if (!expiring.Any(e => e.DocumentId == doc.Id))
                {
                    expiring.Add(new ExpiringDocumentDto
                    {
                        DocumentId = doc.Id,
                        DocumentTypeName = doc.Type.ToString(),
                        ExpiryDate = doc.ExpiryDate.Value,
                        DaysUntilExpiry = doc.ExpiryDate.Value.DayNumber - today.DayNumber,
                        ExpiresDuringTrip = true
                    });
                    warnings.Add($"{doc.Type} expires during your trip on {doc.ExpiryDate:yyyy-MM-dd}.");
                }
            }
        }

        // ---- Deterministic score ----
        // Mandatory contributes 70%, optional 20%, expiry safety 10%
        double mandatoryScore = mandatory.Count == 0 ? 1.0 : (double)mandatoryMet / mandatory.Count;
        double optionalScore = optional.Count == 0 ? 1.0 : (double)optionalMet / optional.Count;

        double expirySafety = 1.0;
        if (blocking.Count > 0) expirySafety -= 0.5 * blocking.Count;
        if (expiring.Any(e => e.DaysUntilExpiry <= ExpiryCriticalDays)) expirySafety -= 0.2;
        expirySafety = Math.Clamp(expirySafety, 0.0, 1.0);

        int score = (int)Math.Round(
            (mandatoryScore * 70.0) +
            (optionalScore * 20.0) +
            (expirySafety * 10.0)
        );
        score = Math.Clamp(score, 0, 100);

        return new ReadinessAssessmentDto
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            Score = score,
            AssessedAt = DateTime.UtcNow,
            MissingDocuments = missing,
            ExpiringDocuments = expiring,
            BlockingIssues = blocking.Distinct().ToList(),
            Warnings = warnings.Distinct().ToList()
        };
    }

    public static bool IsExpired(DocumentEntity doc, DateOnly today)
        => doc.ExpiryDate.HasValue && doc.ExpiryDate.Value < today;

    public static bool ExpiresDuringTrip(DocumentEntity doc, TripEntity trip)
        => doc.ExpiryDate.HasValue &&
           doc.ExpiryDate.Value >= trip.StartDate &&
           doc.ExpiryDate.Value <= trip.EndDate;
}