using Microsoft.EntityFrameworkCore;
using TravelWise.Domain.Entities;
using TravelWise.Domain.Enums;

namespace TravelWise.Infrastructure.Persistence.Seed;

public static class SeedData
{
    public static async Task ApplyAsync(TravelWiseDbContext db, CancellationToken ct = default)
    {
        // Roles
        if (!await db.Roles.AnyAsync(ct))
        {
            db.Roles.AddRange(
                new Role { Name = UserRoleType.Traveller, Description = "Traveller using the mobile app" },
                new Role { Name = UserRoleType.TravelOpsStaff, Description = "Operations staff approving workflows" },
                new Role { Name = UserRoleType.Admin, Description = "System administrator" }
            );
            await db.SaveChangesAsync(ct);
        }

        // Admin user
        if (!await db.Users.AnyAsync(u => u.Email == "admin@travelwise.local", ct))
        {
            var adminRole = await db.Roles.FirstAsync(r => r.Name == UserRoleType.Admin, ct);

            var admin = new User
            {
                Email = "admin@travelwise.local",
                FullName = "TravelWise Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
                IsActive = true
            };
            db.Users.Add(admin);
            await db.SaveChangesAsync(ct);

            db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
            await db.SaveChangesAsync(ct);
        }

        // Sample activities
        if (!await db.Activities.AnyAsync(ct))
        {
            db.Activities.AddRange(
                new Activity
                {
                    Name = "Little Adam's Peak Hike",
                    Category = ActivityCategory.Adventure,
                    Description = "Sunrise hike with panoramic views of Ella Gap.",
                    DurationMinutes = 120,
                    EstimatedCost = 0,
                    Currency = "LKR",
                    Location = "Ella",
                    Latitude = 6.8653,
                    Longitude = 81.0495,
                    IsIndoor = false,
                    Difficulty = "Moderate"
                },
                new Activity
                {
                    Name = "Nine Arches Bridge Visit",
                    Category = ActivityCategory.Sightseeing,
                    Description = "Iconic colonial-era railway bridge surrounded by jungle.",
                    DurationMinutes = 90,
                    EstimatedCost = 500,
                    Currency = "LKR",
                    Location = "Ella",
                    Latitude = 6.8766,
                    Longitude = 81.0607,
                    IsIndoor = false,
                    Difficulty = "Easy"
                },
                new Activity
                {
                    Name = "Ceylon Tea Factory Tour",
                    Category = ActivityCategory.Culture,
                    Description = "Guided tour of a working tea factory with tasting.",
                    DurationMinutes = 75,
                    EstimatedCost = 1500,
                    Currency = "LKR",
                    Location = "Ella",
                    Latitude = 6.8690,
                    Longitude = 81.0450,
                    IsIndoor = true,
                    Difficulty = "Easy"
                },
                new Activity
                {
                    Name = "Ella Gap Viewpoint",
                    Category = ActivityCategory.Nature,
                    Description = "Scenic viewpoint over the Ella Gap with covered seating.",
                    DurationMinutes = 60,
                    EstimatedCost = 300,
                    Currency = "LKR",
                    Location = "Ella",
                    Latitude = 6.8667,
                    Longitude = 81.0467,
                    IsIndoor = true,
                    Difficulty = "Easy"
                },
                new Activity
                {
                    Name = "Ravana Falls Visit",
                    Category = ActivityCategory.Nature,
                    Description = "Waterfall visit, best in dry season.",
                    DurationMinutes = 60,
                    EstimatedCost = 200,
                    Currency = "LKR",
                    Location = "Ella",
                    Latitude = 6.8420,
                    Longitude = 81.0470,
                    IsIndoor = false,
                    Difficulty = "Easy"
                }
            );
            await db.SaveChangesAsync(ct);
        }

        // Sample document requirements (project-defined, not authoritative travel rules)
        if (!await db.DocumentRequirements.AnyAsync(ct))
        {
            db.DocumentRequirements.AddRange(
                new DocumentRequirement { Destination = "Ella", TripType = TripType.Leisure, DocumentType = DocumentType.NationalId, IsMandatory = true, Notes = "Project-defined rule." },
                new DocumentRequirement { Destination = "Ella", TripType = TripType.Leisure, DocumentType = DocumentType.Insurance, IsMandatory = false, Notes = "Recommended." }
            );
            await db.SaveChangesAsync(ct);
        }
    }
}