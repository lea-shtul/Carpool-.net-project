using AutoMapper;
using Carpool.Core.Dtos.Ratings;
using Carpool.Core.Entities;
using Carpool.Core.Enums;
using Carpool.Core.Exceptions;
using Carpool.Core.Interfaces.Repositories;
using Carpool.Core.Interfaces.Services;

namespace Carpool.Service.Services;

/// <summary>
/// Implements <see cref="IRatingService"/> (spec §27–§29). A passenger may rate the driver
/// of a ride only when: the ride is Completed, the reviewer had a Completed booking on it,
/// the reviewer is not the driver, and the reviewer has not already rated this ride. The
/// last rule is also enforced by the unique index on <c>(RideId, ReviewerId)</c>; a rare
/// concurrent duplicate that slips past the pre-check is mapped to 409 by the global
/// exception middleware (spec §28).
/// </summary>
public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IRideRepository _rideRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RatingService(
        IRatingRepository ratingRepository,
        IRideRepository rideRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _ratingRepository = ratingRepository;
        _rideRepository = rideRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RatingResponse> CreateAsync(
        int rideId,
        int currentUserId,
        CreateRatingRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Score is < 1 or > 5)
        {
            throw new BusinessRuleException("Score must be between 1 and 5.");
        }

        var ride = await _rideRepository.GetByIdAsync(rideId, cancellationToken)
            ?? throw new NotFoundException($"Ride {rideId} was not found.");

        if (ride.Status != RideStatus.Completed)
        {
            throw new BusinessRuleException("You can only rate a completed ride.");
        }

        if (ride.DriverId == currentUserId)
        {
            throw new BusinessRuleException("You cannot rate your own ride.");
        }

        var hasCompletedBooking = ride.Bookings.Any(b =>
            b.PassengerId == currentUserId && b.Status == BookingStatus.Completed);
        if (!hasCompletedBooking)
        {
            throw new BusinessRuleException("You must have completed a booking on this ride to rate it.");
        }

        // One rating per (ride, reviewer) — pre-check in front of the unique index (spec §28).
        //here just check without adding the rate
        if (await _ratingRepository.ExistsForReviewerAsync(rideId, currentUserId, cancellationToken))
        {
            throw new ConflictException("You have already rated this ride.");
        }

        var rating = _mapper.Map<Rating>(request);
        rating.RideId = rideId;
        rating.ReviewerId = currentUserId;
        rating.DriverId = ride.DriverId; // denormalised from the ride for driver-rating lookups
        rating.CreatedAt = DateTime.UtcNow;

        await _ratingRepository.AddAsync(rating, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RatingResponse>(rating);
    }

    public async Task<IEnumerable<RatingResponse>> GetForUserAsync(int userId, CancellationToken cancellationToken)
    {
        var ratings = await _ratingRepository.GetByDriverAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<RatingResponse>>(ratings);
    }
}
