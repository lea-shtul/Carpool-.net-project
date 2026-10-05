using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Ratings;

/// <summary>
/// Body of <c>POST /api/rides/{rideId}/ratings</c> (spec §27). The reviewer is
/// taken from the JWT and the ride from the route. Eligibility is checked in the
/// Service layer (ride Completed, reviewer had a Completed booking, not the driver,
/// one rating per ride/reviewer).
/// </summary>
public class CreateRatingRequest
{
    [Range(1, 5)]
    public int Score { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }
}
