using Carpool.Core.Enums;

namespace Carpool.Core.Entities;

/// <summary>
/// An application user (spec §7). Can be a driver, a passenger, or both.
/// Email is unique (enforced via Fluent API index). The password is only ever
/// stored as a hash — never plaintext (spec §7, §39, §60).
/// </summary>
public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Hashed password only. Never exposed through any DTO.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // Navigation properties (one-to-many, spec §32).
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    /// <summary>Rides where this user is the driver (Ride.DriverId).</summary>
    public ICollection<Ride> RidesAsDriver { get; set; } = new List<Ride>();

    /// <summary>Bookings where this user is the passenger (Booking.PassengerId).</summary>
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>Ratings this user has written about drivers (Rating.ReviewerId).</summary>
    public ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();

    /// <summary>Ratings this user has received as a driver (Rating.DriverId).</summary>
    public ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
}
