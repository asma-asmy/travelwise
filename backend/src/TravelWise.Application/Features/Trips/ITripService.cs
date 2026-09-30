using TravelWise.Application.Features.Trips.Dtos;

namespace TravelWise.Application.Features.Trips;

public interface ITripService
{
    /// <summary>Trips owned by the given user. Always owner-scoped, regardless of role.</summary>
    Task<List<TripDto>> ListMyTripsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>All trips in the system. Staff/Admin only.</summary>
    Task<List<TripDto>> ListAllTripsAsync(CancellationToken ct = default);

    /// <summary>Single trip by ID. Staff can access any; travellers only their own.</summary>
    Task<TripDto> GetAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default);
}