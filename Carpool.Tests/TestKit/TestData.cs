using Carpool.Core.Entities;
using Carpool.Core.Enums;

namespace Carpool.Tests.TestKit;

/// <summary>Concise builders for the entities used across the service tests.</summary>
public static class TestData
{
    public static User User(
        int id,
        UserRole role = UserRole.User,
        bool isActive = true,
        string? email = null) => new()
    {
        Id = id,
        FirstName = $"First{id}",
        LastName = $"Last{id}",
        Email = email ?? $"user{id}@test.local",
        PasswordHash = "hash",
        PhoneNumber = "+10000000000",
        Role = role,
        IsActive = isActive,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    public static Vehicle Vehicle(int id, int ownerId, int passengerCapacity = 4) => new()
    {
        Id = id,
        OwnerId = ownerId,
        Manufacturer = "Make",
        Model = "Model",
        LicensePlate = $"PLATE-{id}",
        PassengerCapacity = passengerCapacity,
    };

    public static Ride Ride(
        int id,
        int driverId,
        int vehicleId = 1,
        RideStatus status = RideStatus.Scheduled,
        int totalSeats = 4,
        int? availableSeats = null,
        DateTime? departureTime = null,
        int estimatedDurationMinutes = 60) => new()
    {
        Id = id,
        DriverId = driverId,
        VehicleId = vehicleId,
        Origin = "Origin",
        Destination = "Destination",
        DepartureTime = departureTime ?? DateTime.UtcNow.AddHours(2),
        TotalSeats = totalSeats,
        AvailableSeats = availableSeats ?? totalSeats,
        PricePerSeat = 10m,
        EstimatedDurationMinutes = estimatedDurationMinutes,
        Status = status,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        Bookings = new List<Booking>(),
        RideTags = new List<RideTag>(),
        // Minimal navs so Ride -> RideResponse mapping works in tests that assert on the DTO.
        Driver = User(driverId),
        Vehicle = Vehicle(vehicleId, driverId),
    };

    public static Booking Booking(
        int id,
        int rideId,
        int passengerId,
        int numberOfSeats = 1,
        BookingStatus status = BookingStatus.Active) => new()
    {
        Id = id,
        RideId = rideId,
        PassengerId = passengerId,
        NumberOfSeats = numberOfSeats,
        Status = status,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        // Minimal nav so Booking -> RideBookingResponse mapping works in tests that assert on the DTO.
        Passenger = User(passengerId),
    };
}
