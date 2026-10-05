using Carpool.Core.Entities;
using Carpool.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Carpool.Data.Seed;

/// <summary>
/// Deterministic seed data applied through EF Core's <c>HasData</c> (spec §49) so the
/// project runs immediately after <c>database update</c>. Every value is static — fixed
/// primary keys, a fixed UTC base timestamp (spec §50) and a pre-computed password hash —
/// so a model diff does not change between migrations.
///
/// This is applied as its own migration (<c>SeedData</c>), separate from the schema
/// migration, which also satisfies the "at least 2 migrations" requirement of spec §47.
///
/// All demo users share the password <c>Passw0rd!</c> (documented in the README, not a
/// real secret). <see cref="DemoPasswordHash"/> was generated once with
/// <c>Microsoft.AspNetCore.Identity.PasswordHasher&lt;TUser&gt;</c> default options
/// (PBKDF2, Identity V3). That format embeds its salt, PRF and iteration count, so it
/// verifies against the same hasher wired up in Stage 8.
/// </summary>
public static class SeedData
{
    private static readonly DateTime CreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Far enough in the future that the Stage 13 <c>BackgroundService</c> leaves these
    /// seeded rides in the <see cref="RideStatus.Scheduled"/> state.
    /// </summary>
    private static readonly DateTime DepartureTime = new(2030, 1, 1, 8, 0, 0, DateTimeKind.Utc);

    private const string DemoPasswordHash =
        "AQAAAAIAAYagAAAAEBUbLMKM4BpncNZwJpDpoOdp7Iyxiec5eZYskgkZMkOCzUATnykIFmg52+g+igRoUQ==";

    public static void Seed(ModelBuilder modelBuilder)
    {
        // Predefined tags (spec §30). Retrieval-only via GET /api/tags.
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Quiet" },
            new Tag { Id = 2, Name = "Music" },
            new Tag { Id = 3, Name = "PetsAllowed" },
            new Tag { Id = 4, Name = "SmokingForbidden" });

        // One admin + three regular users (spec §49). Passwords stored hashed only.
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, FirstName = "Admin", LastName = "User", Email = "admin@carpool.dev", PasswordHash = DemoPasswordHash, PhoneNumber = "+972500000001", Role = UserRole.Admin, IsActive = true, CreatedAt = CreatedAt },
            new User { Id = 2, FirstName = "Alice", LastName = "Cohen", Email = "alice@carpool.dev", PasswordHash = DemoPasswordHash, PhoneNumber = "+972500000002", Role = UserRole.User, IsActive = true, CreatedAt = CreatedAt },
            new User { Id = 3, FirstName = "Bob", LastName = "Levi", Email = "bob@carpool.dev", PasswordHash = DemoPasswordHash, PhoneNumber = "+972500000003", Role = UserRole.User, IsActive = true, CreatedAt = CreatedAt },
            new User { Id = 4, FirstName = "Carol", LastName = "Mizrahi", Email = "carol@carpool.dev", PasswordHash = DemoPasswordHash, PhoneNumber = "+972500000004", Role = UserRole.User, IsActive = true, CreatedAt = CreatedAt });

        // One vehicle each for the three regular users (spec §49). The admin has none.
        modelBuilder.Entity<Vehicle>().HasData(
            new Vehicle { Id = 1, OwnerId = 2, Manufacturer = "Toyota", Model = "Corolla", LicensePlate = "111-11-111", PassengerCapacity = 4 },
            new Vehicle { Id = 2, OwnerId = 3, Manufacturer = "Honda", Model = "Civic", LicensePlate = "222-22-222", PassengerCapacity = 3 },
            new Vehicle { Id = 3, OwnerId = 4, Manufacturer = "Mazda", Model = "3", LicensePlate = "333-33-333", PassengerCapacity = 5 });

        // Two example Scheduled rides (spec §49). TotalSeats <= the vehicle's capacity
        // (spec §11); AvailableSeats == TotalSeats because there are no seeded bookings.
        modelBuilder.Entity<Ride>().HasData(
            new Ride { Id = 1, DriverId = 2, VehicleId = 1, Origin = "Tel Aviv", Destination = "Jerusalem", DepartureTime = DepartureTime, TotalSeats = 3, AvailableSeats = 3, PricePerSeat = 25.00m, EstimatedDurationMinutes = 60, Status = RideStatus.Scheduled, CreatedAt = CreatedAt },
            new Ride { Id = 2, DriverId = 3, VehicleId = 2, Origin = "Haifa", Destination = "Tel Aviv", DepartureTime = DepartureTime, TotalSeats = 2, AvailableSeats = 2, PricePerSeat = 30.00m, EstimatedDurationMinutes = 90, Status = RideStatus.Scheduled, CreatedAt = CreatedAt });

        // Ride ↔ Tag many-to-many join rows (spec §31).
        modelBuilder.Entity<RideTag>().HasData(
            new RideTag { RideId = 1, TagId = 1 },   // Tel Aviv → Jerusalem: Quiet
            new RideTag { RideId = 1, TagId = 4 },   //                        SmokingForbidden
            new RideTag { RideId = 2, TagId = 2 });  // Haifa → Tel Aviv:      Music
    }
}
