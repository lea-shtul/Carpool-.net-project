using Carpool.Core.Enums;
using Carpool.Core.Exceptions;
using Carpool.Core.Interfaces.Repositories;
using Carpool.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Carpool.Service.Services;

/// <summary>
/// Implements <see cref="IRideStatusService"/> (spec §15). Each due ride is transitioned in
/// its own transaction so a concurrent seat booking that bumps one ride's <c>xmin</c> only
/// makes that ride retry on the next tick — the rest still process. A genuine infrastructure
/// failure is allowed to propagate so the hosted service logs it as an error.
/// </summary>
public class RideStatusService : IRideStatusService
{
    private readonly IRideRepository _rideRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RideStatusService> _logger;

    public RideStatusService(
        IRideRepository rideRepository,
        IUnitOfWork unitOfWork,
        ILogger<RideStatusService> logger)
    {
        _rideRepository = rideRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> StartDueRidesAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var dueRides = await _rideRepository.GetRidesDueToStartAsync(now, cancellationToken);

        var transitioned = 0;
        foreach (var ride in dueRides)
        {
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    ride.Status = RideStatus.InProgress;
                    ride.StartedAt = now;
                    await _unitOfWork.SaveChangesAsync(token);
                }, cancellationToken);

                transitioned++;
            }
            catch (ConcurrencyConflictException)
            {
                _logger.LogWarning(
                    "Ride {RideId} was modified concurrently while starting it; will retry on the next tick.",
                    ride.Id);
            }
        }

        return transitioned;
    }

    public async Task<int> CompleteDueRidesAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var dueRides = await _rideRepository.GetRidesDueToCompleteAsync(now, cancellationToken);

        var transitioned = 0;
        foreach (var ride in dueRides)
        {
            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async token =>
                {
                    ride.Status = RideStatus.Completed;
                    ride.CompletedAt = now;
                    ride.StartedAt ??= now;

                    // All active bookings complete in the same transaction (spec §14).
                    foreach (var booking in ride.Bookings.Where(b => b.Status == BookingStatus.Active))
                    {
                        booking.Status = BookingStatus.Completed;
                    }

                    await _unitOfWork.SaveChangesAsync(token);
                }, cancellationToken);

                transitioned++;
            }
            catch (ConcurrencyConflictException)
            {
                _logger.LogWarning(
                    "Ride {RideId} was modified concurrently while completing it; will retry on the next tick.",
                    ride.Id);
            }
        }

        return transitioned;
    }
}
