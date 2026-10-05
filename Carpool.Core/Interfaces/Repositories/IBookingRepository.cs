using Carpool.Core.Entities;

namespace Carpool.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="Booking"/> (spec §21, §22).</summary>
public interface IBookingRepository
{
    /// <summary>Tracked load including the parent <c>Ride</c>, for the cancel flow (spec §25).</summary>
    Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IEnumerable<Booking>> GetByPassengerAsync(int passengerId, CancellationToken cancellationToken);

    /// <summary>True if the passenger already has an Active booking on the ride (spec §26).</summary>
    Task<bool> HasActiveBookingAsync(int rideId, int passengerId, CancellationToken cancellationToken);

    /// <summary>All Active bookings for a ride — used when a ride is cancelled or completed (spec §14, §18).</summary>
    Task<IEnumerable<Booking>> GetActiveByRideAsync(int rideId, CancellationToken cancellationToken);

    Task AddAsync(Booking booking, CancellationToken cancellationToken);

    void Update(Booking booking);
}
