namespace Carpool.Core.Dtos.Ratings;

/// <summary>Public projection of a <see cref="Entities.Rating"/> (spec §33, §29).</summary>
public class RatingResponse
{
    public int Id { get; set; }

    public int RideId { get; set; }

    public int ReviewerId { get; set; }

    public int DriverId { get; set; }

    public int Score { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}
