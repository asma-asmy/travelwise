using TravelWise.Application.Features.Budget.Dtos;   // PagedResult<T>
using TravelWise.Application.Features.Risk.Dtos;

namespace TravelWise.Application.Features.Risk;

public interface IRiskService
{
    // Alerts (admin)
    Task<PagedResult<SafetyAlertDto>> ListAlertsAsync(SafetyAlertQueryDto query, CancellationToken ct = default);
    Task<SafetyAlertDto> CreateAlertAsync(CreateSafetyAlertDto dto, CancellationToken ct = default);
    Task<SafetyAlertDto> UpdateAlertAsync(Guid id, UpdateSafetyAlertDto dto, CancellationToken ct = default);
    Task DeleteAlertAsync(Guid id, CancellationToken ct = default);

    // Destination risk
    Task<DestinationRiskDto> GetDestinationRiskAsync(string destination, CancellationToken ct = default);

    // Weather (proxied via backend)
    Task<WeatherDto?> GetWeatherAsync(string destination, DateOnly date, CancellationToken ct = default);

    // Safety information
    Task<SafetyInformationDto> GetSafetyInformationAsync(string destination, CancellationToken ct = default);

    // Business operations
    Task<RiskAssessmentDto> AssessTripAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default);
    Task<RiskAssessmentDto?> GetLatestAssessmentAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default);
}