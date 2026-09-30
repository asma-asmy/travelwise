using Microsoft.EntityFrameworkCore;
using TravelWise.Application.Common.Exceptions;
using TravelWise.Application.Features.Trips;
using TravelWise.Application.Features.Trips.Dtos;
using TravelWise.Infrastructure.Persistence;
using TripEntity = TravelWise.Domain.Entities.Trip;

namespace TravelWise.Infrastructure.Services.Trips;

public class TripService : ITripService
{
    private readonly TravelWiseDbContext _db;

    public TripService(TravelWiseDbContext db)
    {
        _db = db;
    }

    public async Task<List<TripDto>> ListMyTripsAsync(Guid userId, CancellationToken ct = default)
    {
        // Owner-scoped: "My Trips" is always the current user's own trips.
        var trips = await _db.Trips
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(ct);

        return trips.Select(Map).ToList();
    }

    public async Task<List<TripDto>> ListAllTripsAsync(CancellationToken ct = default)
    {
        // Cross-user: staff/admin oversight view.
        var trips = await _db.Trips
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(ct);

        return trips.Select(Map).ToList();
    }

    public async Task<TripDto> GetAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct = default)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct)
            ?? throw new NotFoundException("Trip not found.");

        if (!isStaff && trip.UserId != userId)
            throw new ForbiddenException("You do not have access to this trip.");

        return Map(trip);
    }

    private static TripDto Map(TripEntity t) => new()
    {
        Id = t.Id,
        UserId = t.UserId,
        Title = t.Title,
        Destination = t.Destination,
        Origin = t.Origin,
        StartDate = t.StartDate,
        EndDate = t.EndDate,
        Currency = t.Currency,
        TripType = t.TripType.ToString(),
        Status = t.Status.ToString(),
        TravellerCount = t.TravellerCount,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}