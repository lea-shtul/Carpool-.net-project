using AutoMapper;
using Carpool.Core.Dtos.Rides;
using Carpool.Core.Entities;
using Carpool.Core.Enums;
using Carpool.Core.Exceptions;
using Carpool.Core.Interfaces.Repositories;
using Carpool.Service.Services;
using Carpool.Tests.TestKit;
using Moq;

namespace Carpool.Tests.Services;

/// <summary>
/// Business-rule tests for <see cref="RideService"/> — covers ride cancellation and ride
/// completion (spec §70) with their booking cascades, plus the creation guards
/// (vehicle ownership, seat capacity, tag validity).
/// </summary>
public class RideServiceTests
{
    private const int Driver = 1;
    private const int Other = 2;
    private const int VehicleId = 5;
    private const int RideId = 10;

    private readonly Mock<IRideRepository> _rides = new();
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<ITagRepository> _tags = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = TestMocks.UnitOfWork();
    private readonly IMapper _mapper = TestMapper.Create();

    private RideService CreateSut() => new(_rides.Object, _vehicles.Object, _tags.Object, _unitOfWork.Object, _mapper);

    private void RideLoads(Ride ride)
    {
        _rides.Setup(r => r.GetByIdAsync(ride.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ride);
        _rides.Setup(r => r.GetDetailedByIdAsync(ride.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ride);
    }

    // --- CancelAsync ---------------------------------------------------------------

    [Fact]
    public async Task CancelAsync_WhenScheduledAndCallerIsDriver_CancelsRideAndActiveBookings()
    {
        var ride = TestData.Ride(RideId, Driver, status: RideStatus.Scheduled);
        ride.Bookings = new List<Booking>
        {
            TestData.Booking(1, RideId, 20, status: BookingStatus.Active),
            TestData.Booking(2, RideId, 21, status: BookingStatus.Active),
            TestData.Booking(3, RideId, 22, status: BookingStatus.Cancelled),
        };
        RideLoads(ride);

        await CreateSut().CancelAsync(RideId, Driver, UserRole.User, default);

        Assert.Equal(RideStatus.Cancelled, ride.Status);
        Assert.All(ride.Bookings.Where(b => b.Id != 3), b =>
        {
            Assert.Equal(BookingStatus.Cancelled, b.Status);
            Assert.NotNull(b.CancelledAt);
        });
        Assert.Equal(BookingStatus.Cancelled, ride.Bookings.Single(b => b.Id == 3).Status); // untouched
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_AsAdmin_CanCancelAnotherDriversRide()
    {
        var ride = TestData.Ride(RideId, Driver, status: RideStatus.Scheduled);
        RideLoads(ride);

        await CreateSut().CancelAsync(RideId, Other, UserRole.Admin, default);

        Assert.Equal(RideStatus.Cancelled, ride.Status);
    }

    [Fact]
    public async Task CancelAsync_WhenCallerIsNotDriverNorAdmin_ThrowsForbidden()
    {
        var ride = TestData.Ride(RideId, Driver, status: RideStatus.Scheduled);
        RideLoads(ride);

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateSut().CancelAsync(RideId, Other, UserRole.User, default));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(RideStatus.InProgress)]
    [InlineData(RideStatus.Completed)]
    [InlineData(RideStatus.Cancelled)]
    public async Task CancelAsync_WhenRideNotScheduled_ThrowsBusinessRule(RideStatus status)
    {
        var ride = TestData.Ride(RideId, Driver, status: status);
        RideLoads(ride);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateSut().CancelAsync(RideId, Driver, UserRole.User, default));
    }

    [Fact]
    public async Task CancelAsync_WhenRideNotFound_ThrowsNotFound()
    {
        _rides.Setup(r => r.GetByIdAsync(RideId, It.IsAny<CancellationToken>())).ReturnsAsync((Ride?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateSut().CancelAsync(RideId, Driver, UserRole.User, default));
    }

    // --- CompleteAsync ------------------------------------------------------------

    [Fact]
    public async Task CompleteAsync_WhenInProgress_CompletesRideAndActiveBookings()
    {
        var ride = TestData.Ride(RideId, Driver, status: RideStatus.InProgress);
        ride.Bookings = new List<Booking>
        {
            TestData.Booking(1, RideId, 20, status: BookingStatus.Active),
            TestData.Booking(2, RideId, 21, status: BookingStatus.Cancelled),
        };
        RideLoads(ride);

        await CreateSut().CompleteAsync(RideId, Driver, UserRole.User, default);

        Assert.Equal(RideStatus.Completed, ride.Status);
        Assert.NotNull(ride.CompletedAt);
        Assert.NotNull(ride.StartedAt);
        Assert.Equal(BookingStatus.Completed, ride.Bookings.Single(b => b.Id == 1).Status);
        Assert.Equal(BookingStatus.Cancelled, ride.Bookings.Single(b => b.Id == 2).Status); // untouched
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_WhenScheduledButDeparted_IsAllowed()
    {
        var ride = TestData.Ride(RideId, Driver, status: RideStatus.Scheduled,
            departureTime: DateTime.UtcNow.AddMinutes(-30));
        RideLoads(ride);

        await CreateSut().CompleteAsync(RideId, Driver, UserRole.User, default);

        Assert.Equal(RideStatus.Completed, ride.Status);
        Assert.NotNull(ride.StartedAt);
        Assert.NotNull(ride.CompletedAt);
    }

    [Fact]
    public async Task CompleteAsync_WhenScheduledAndNotYetDeparted_ThrowsBusinessRule()
    {
        var ride = TestData.Ride(RideId, Driver, status: RideStatus.Scheduled,
            departureTime: DateTime.UtcNow.AddHours(3));
        RideLoads(ride);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateSut().CompleteAsync(RideId, Driver, UserRole.User, default));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(RideStatus.Completed)]
    [InlineData(RideStatus.Cancelled)]
    public async Task CompleteAsync_WhenAlreadyFinished_ThrowsBusinessRule(RideStatus status)
    {
        var ride = TestData.Ride(RideId, Driver, status: status);
        RideLoads(ride);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateSut().CompleteAsync(RideId, Driver, UserRole.User, default));
    }

    [Fact]
    public async Task CompleteAsync_WhenCallerIsNotDriverNorAdmin_ThrowsForbidden()
    {
        var ride = TestData.Ride(RideId, Driver, status: RideStatus.InProgress);
        RideLoads(ride);

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateSut().CompleteAsync(RideId, Other, UserRole.User, default));
    }

    // --- CreateAsync -------------------------------------------------------------

    private CreateRideRequest ValidCreateRequest(int totalSeats = 3, List<int>? tagIds = null) => new()
    {
        VehicleId = VehicleId,
        Origin = "A",
        Destination = "B",
        DepartureTime = DateTime.UtcNow.AddDays(1),
        TotalSeats = totalSeats,
        PricePerSeat = 12m,
        EstimatedDurationMinutes = 45,
        TagIds = tagIds ?? new List<int>(),
    };

    [Fact]
    public async Task CreateAsync_WithOwnedVehicleAndValidSeats_InitialisesRide()
    {
        _vehicles.Setup(v => v.GetByIdAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Vehicle(VehicleId, Driver, passengerCapacity: 4));
        _rides.Setup(r => r.GetDetailedByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Ride(RideId, Driver, VehicleId));

        Ride? added = null;
        _rides.Setup(r => r.AddAsync(It.IsAny<Ride>(), It.IsAny<CancellationToken>()))
            .Callback<Ride, CancellationToken>((r, _) => added = r)
            .Returns(Task.CompletedTask);

        await CreateSut().CreateAsync(Driver, ValidCreateRequest(totalSeats: 3), default);

        Assert.NotNull(added);
        Assert.Equal(Driver, added!.DriverId);
        Assert.Equal(3, added.TotalSeats);
        Assert.Equal(3, added.AvailableSeats);
        Assert.Equal(RideStatus.Scheduled, added.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenVehicleNotOwnedByCaller_ThrowsForbidden()
    {
        _vehicles.Setup(v => v.GetByIdAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Vehicle(VehicleId, ownerId: Other));

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateSut().CreateAsync(Driver, ValidCreateRequest(), default));
        _rides.Verify(r => r.AddAsync(It.IsAny<Ride>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenVehicleNotFound_ThrowsNotFound()
    {
        _vehicles.Setup(v => v.GetByIdAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateSut().CreateAsync(Driver, ValidCreateRequest(), default));
    }

    [Fact]
    public async Task CreateAsync_WhenTotalSeatsExceedVehicleCapacity_ThrowsBusinessRule()
    {
        _vehicles.Setup(v => v.GetByIdAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Vehicle(VehicleId, Driver, passengerCapacity: 2));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().CreateAsync(Driver, ValidCreateRequest(totalSeats: 3), default));
    }

    [Fact]
    public async Task CreateAsync_WhenAnyTagIdIsUnknown_ThrowsBusinessRule()
    {
        _vehicles.Setup(v => v.GetByIdAsync(VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Vehicle(VehicleId, Driver, passengerCapacity: 4));
        _tags.Setup(t => t.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tag> { new() { Id = 1, Name = "Quiet" } }); // only 1 of 2 resolves

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().CreateAsync(Driver, ValidCreateRequest(tagIds: new List<int> { 1, 99 }), default));
        _rides.Verify(r => r.AddAsync(It.IsAny<Ride>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
