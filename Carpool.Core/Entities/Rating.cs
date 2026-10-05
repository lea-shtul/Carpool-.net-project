namespace Carpool.Core.Entities;

/// <summary>
/// A passenger's rating of the driver for a completed ride (spec §27–§29).
/// Eligibility (enforced in the Service layer): the ride is Completed, the reviewer
/// had a Completed booking on it, the reviewer is not the driver, and there is at
/// most one rating per (ride, reviewer) — the last also enforced by a unique index
/// on <c>(RideId, ReviewerId)</c> so concurrent requests cannot both succeed (spec §28).
/// </summary>
public class Rating
{
    public int Id { get; set; }

    /// <summary>FK to the rated <see cref="Ride"/>.</summary>
    public int RideId { get; set; }

    /// <summary>FK to the passenger who wrote the rating (<see cref="User"/>).</summary>
    public int ReviewerId { get; set; }

    /// <summary>FK to the driver being rated (<see cref="User"/>). Denormalised from the ride for easy lookup of a driver's ratings.</summary>
    public int DriverId { get; set; }

    /// <summary>Score, validated to the 1–5 range in the Service layer and via Data Annotations on the request DTO.</summary>
    public int Score { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties.
    public Ride Ride { get; set; } = null!;

    public User Reviewer { get; set; } = null!;

    public User Driver { get; set; } = null!;
}
