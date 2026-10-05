using Carpool.Core.Enums;

namespace Carpool.Core.Dtos.Bookings;

/// <summary>Public projection of a <see cref="Entities.Booking"/> (spec §33).</summary>
public class BookingResponse
{
    public int Id { get; set; }

    public int RideId { get; set; }

    public int PassengerId { get; set; }

    public int NumberOfSeats { get; set; }

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }
}
