using AutoMapper;
using Carpool.Core.Dtos.Ratings;
using Carpool.Core.Entities;
using Carpool.Core.Enums;
using Carpool.Core.Exceptions;
using Carpool.Core.Interfaces.Repositories;
using Carpool.Service.Services;
using Carpool.Tests.TestKit;
using Moq;

namespace Carpool.Tests.Services;

/// <summary>
/// Rating-eligibility tests for <see cref="RatingService"/> (spec §70): the ride must be
/// Completed, the reviewer must have had a Completed booking on it, the reviewer cannot be
/// the driver, and there can be only one rating per (ride, reviewer).
/// </summary>
public class RatingServiceTests
{
    private const int Driver = 1;
    private const int Reviewer = 2;
    private const int RideId = 10;

    private readonly Mock<IRatingRepository> _ratings = new();
    private readonly Mock<IRideRepository> _rides = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = TestMocks.UnitOfWork();
    private readonly IMapper _mapper = TestMapper.Create();

    private RatingService CreateSut() => new(_ratings.Object, _rides.Object, _unitOfWork.Object, _mapper);

    private Ride CompletedRideWithReviewerBooking()
    {
        var ride = TestData.Ride(RideId, Driver, status: RideStatus.Completed);
        ride.Bookings = new List<Booking>
        {
            TestData.Booking(1, RideId, Reviewer, status: BookingStatus.Completed),
        };
        return ride;
    }

    private CreateRatingRequest Request(int score = 5, string? comment = "Great ride") => new()
    {
        Score = score,
        Comment = comment,
    };

    [Fact]
    public async Task CreateAsync_WhenEligible_AddsRatingWithDriverCopiedFromRide()
    {
        var ride = CompletedRideWithReviewerBooking();
        _rides.Setup(r => r.GetByIdAsync(RideId, It.IsAny<CancellationToken>())).ReturnsAsync(ride);
        _ratings.Setup(r => r.ExistsForReviewerAsync(RideId, Reviewer, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        Rating? added = null;
        _ratings.Setup(r => r.AddAsync(It.IsAny<Rating>(), It.IsAny<CancellationToken>()))
            .Callback<Rating, CancellationToken>((r, _) => added = r)
            .Returns(Task.CompletedTask);

        var result = await CreateSut().CreateAsync(RideId, Reviewer, Request(score: 4), default);

        Assert.NotNull(added);
        Assert.Equal(RideId, added!.RideId);
        Assert.Equal(Reviewer, added.ReviewerId);
        Assert.Equal(Driver, added.DriverId);
        Assert.Equal(4, added.Score);
        Assert.Equal(4, result.Score);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CreateAsync_WhenScoreOutOfRange_ThrowsBusinessRule(int score)
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().CreateAsync(RideId, Reviewer, Request(score: score), default));
    }

    [Fact]
    public async Task CreateAsync_WhenRideNotFound_ThrowsNotFound()
    {
        _rides.Setup(r => r.GetByIdAsync(RideId, It.IsAny<CancellationToken>())).ReturnsAsync((Ride?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateSut().CreateAsync(RideId, Reviewer, Request(), default));
    }

    [Theory]
    [InlineData(RideStatus.Scheduled)]
    [InlineData(RideStatus.InProgress)]
    [InlineData(RideStatus.Cancelled)]
    public async Task CreateAsync_WhenRideNotCompleted_ThrowsBusinessRule(RideStatus status)
    {
        var ride = TestData.Ride(RideId, Driver, status: status);
        _rides.Setup(r => r.GetByIdAsync(RideId, It.IsAny<CancellationToken>())).ReturnsAsync(ride);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().CreateAsync(RideId, Reviewer, Request(), default));
    }

    [Fact]
    public async Task CreateAsync_WhenReviewerIsTheDriver_ThrowsBusinessRule()
    {
        var ride = CompletedRideWithReviewerBooking();
        _rides.Setup(r => r.GetByIdAsync(RideId, It.IsAny<CancellationToken>())).ReturnsAsync(ride);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().CreateAsync(RideId, currentUserId: Driver, Request(), default));
    }

    [Fact]
    public async Task CreateAsync_WhenReviewerHasNoCompletedBookingOnTheRide_ThrowsBusinessRule()
    {
        var ride = TestData.Ride(RideId, Driver, status: RideStatus.Completed);
        ride.Bookings = new List<Booking>
        {
            TestData.Booking(1, RideId, Reviewer, status: BookingStatus.Cancelled), // not Completed
            TestData.Booking(2, RideId, 99, status: BookingStatus.Completed),        // someone else
        };
        _rides.Setup(r => r.GetByIdAsync(RideId, It.IsAny<CancellationToken>())).ReturnsAsync(ride);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().CreateAsync(RideId, Reviewer, Request(), default));
    }

    [Fact]
    public async Task CreateAsync_WhenReviewerAlreadyRatedTheRide_ThrowsConflict()
    {
        var ride = CompletedRideWithReviewerBooking();
        _rides.Setup(r => r.GetByIdAsync(RideId, It.IsAny<CancellationToken>())).ReturnsAsync(ride);
        _ratings.Setup(r => r.ExistsForReviewerAsync(RideId, Reviewer, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateSut().CreateAsync(RideId, Reviewer, Request(), default));
        _ratings.Verify(r => r.AddAsync(It.IsAny<Rating>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetForUserAsync_ReturnsMappedRatings()
    {
        _ratings.Setup(r => r.GetByDriverAsync(Driver, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Rating>
            {
                new() { Id = 1, RideId = RideId, ReviewerId = Reviewer, DriverId = Driver, Score = 5, CreatedAt = DateTime.UtcNow },
                new() { Id = 2, RideId = 11, ReviewerId = 3, DriverId = Driver, Score = 3, CreatedAt = DateTime.UtcNow },
            });

        var result = (await CreateSut().GetForUserAsync(Driver, default)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == 1 && x.Score == 5);
    }
}
