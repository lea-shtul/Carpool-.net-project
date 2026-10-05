using AutoMapper;
using Carpool.Core.Dtos.Bookings;
using Carpool.Core.Entities;
using Carpool.Core.Enums;
using Carpool.Core.Exceptions;
using Carpool.Core.Interfaces.Repositories;
using Carpool.Core.Interfaces.Services;

namespace Carpool.Service.Services;

/// <summary>
/// Implements <see cref="IBookingService"/> (spec §22–§26). The ride is a limited resource:
/// <see cref="CreateAsync"/> and <see cref="CancelAsync"/> change <c>Ride.AvailableSeats</c>
/// on the tracked ride inside a transaction, so the ride's <c>xmin</c> concurrency token
/// guards the update. A concurrent seat change makes <c>SaveChangesAsync</c> raise
/// <c>DbUpdateConcurrencyException</c>, which <c>CarpoolUnitOfWork</c> translates to
/// <see cref="ConcurrencyConflictException"/> → HTTP 409. Seats can never go negative
/// (validated here and backed by a database check constraint) (spec §23, §24).
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRideRepository _rideRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BookingService(
        IBookingRepository bookingRepository,
        IRideRepository rideRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _rideRepository = rideRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BookingResponse> CreateAsync(
        int rideId,
        int currentUserId,
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        if (request.NumberOfSeats <= 0)
        {
            throw new BusinessRuleException("NumberOfSeats must be greater than zero.");
        }

        var ride = await _rideRepository.GetByIdAsync(rideId, cancellationToken)
            ?? throw new NotFoundException($"Ride {rideId} was not found.");

        // Ride must be bookable: Scheduled, not departed, not cancelled (spec §22).
        if (ride.Status != RideStatus.Scheduled)
        {
            throw new BusinessRuleException("This ride is not open for booking.");
        }

        if (ride.DepartureTime <= DateTime.UtcNow)
        {
            throw new BusinessRuleException("The ride has already departed.");
        }

        if (ride.DriverId == currentUserId)
        {
            throw new BusinessRuleException("You cannot book your own ride.");
        }

        // At most one Active booking per (ride, passenger) — also enforced by a filtered
        // unique index (spec §26).
        if (await _bookingRepository.HasActiveBookingAsync(rideId, currentUserId, cancellationToken))
        {
            throw new ConflictException("You already have an active booking for this ride.");
        }

        if (ride.AvailableSeats < request.NumberOfSeats)
        {
            throw new BusinessRuleException("Not enough available seats on this ride.");
        }

        var booking = _mapper.Map<Booking>(request);
        booking.RideId = rideId;
        booking.PassengerId = currentUserId;
        booking.Status = BookingStatus.Active;
        booking.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            // Tracked ride: this UPDATE carries `WHERE xmin = <original>`. If another
            // request booked first, 0 rows match → DbUpdateConcurrencyException → 409.
            ride.AvailableSeats -= request.NumberOfSeats;
            await _bookingRepository.AddAsync(booking, token);
            await _unitOfWork.SaveChangesAsync(token);
        }, cancellationToken);

        return _mapper.Map<BookingResponse>(booking);
    }

    public async Task<IEnumerable<BookingResponse>> GetMineAsync(int currentUserId, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByPassengerAsync(currentUserId, cancellationToken);
        return _mapper.Map<IEnumerable<BookingResponse>>(bookings);
    }

    public async Task<BookingResponse> GetByIdAsync(
        int bookingId,
        int currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException($"Booking {bookingId} was not found.");

        if (booking.PassengerId != currentUserId && currentUserRole != UserRole.Admin)
        {
            throw new ForbiddenException("You do not have permission to access this booking.");
        }

        return _mapper.Map<BookingResponse>(booking);
    }

    public async Task<BookingResponse> CancelAsync(int bookingId, int currentUserId, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException($"Booking {bookingId} was not found.");

        // Only the booking's own passenger may cancel it — no Admin bypass (spec §25).
        if (booking.PassengerId != currentUserId)
        {
            throw new ForbiddenException("You can only cancel your own booking.");
        }

        // Allowed only while the booking is Active and the ride is still Scheduled (spec §25).
        if (booking.Status != BookingStatus.Active || booking.Ride.Status != RideStatus.Scheduled)
        {
            throw new BusinessRuleException("This booking can no longer be cancelled.");
        }

        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;

            // Return the seats to the ride. Tracked ride → xmin-guarded UPDATE (spec §25).
            booking.Ride.AvailableSeats += booking.NumberOfSeats;

            await _unitOfWork.SaveChangesAsync(token);
        }, cancellationToken);

        return _mapper.Map<BookingResponse>(booking);
    }
}
