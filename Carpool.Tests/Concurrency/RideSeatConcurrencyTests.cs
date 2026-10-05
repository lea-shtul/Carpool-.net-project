using Xunit;
using Carpool.Core.Exceptions;
using Carpool.Data.Repositories;
using Carpool.Tests.TestKit;
using Microsoft.EntityFrameworkCore;

namespace Carpool.Tests.Concurrency;

/// <summary>
/// The optimistic-concurrency test required by spec §69. Two separate <c>DbContext</c>
/// instances read the same ride and both try to change its available seats: the first save
/// wins, the second fails because the PostgreSQL <c>xmin</c> token has moved. This is the
/// project's central limited-resource guarantee — seats can never go negative and two
/// bookings can never both succeed against the same last seat.
/// </summary>
[Trait("Category", "Integration")]
[Collection(PostgresIntegrationCollection.Name)]
public class RideSeatConcurrencyTests
{
    private const int SeededRideId = 1;

    private readonly PostgresTestDatabase _database;

    public RideSeatConcurrencyTests(PostgresTestDatabase database) => _database = database;

    private async Task ResetSeatsAsync(int availableSeats)
    {
        await using var context = _database.NewContext();
        var ride = await context.Rides.SingleAsync(r => r.Id == SeededRideId);
        ride.AvailableSeats = availableSeats;
        ride.TotalSeats = Math.Max(ride.TotalSeats, availableSeats);
        await context.SaveChangesAsync();
    }

    [SkippableFact]
    public async Task TwoContexts_BothDecrementSameRideSeats_SecondSaveThrowsDbUpdateConcurrencyException()
    {
        Skip.IfNot(_database.Available, $"PostgreSQL not available: {_database.UnavailableReason}");

        await ResetSeatsAsync(3);

        await using var contextA = _database.NewContext();
        await using var contextB = _database.NewContext();

        var rideA = await contextA.Rides.SingleAsync(r => r.Id == SeededRideId);
        var rideB = await contextB.Rides.SingleAsync(r => r.Id == SeededRideId);

        // Both contexts have read the same starting value.
        Assert.Equal(3, rideA.AvailableSeats);
        Assert.Equal(3, rideB.AvailableSeats);

        // First writer succeeds.
        rideA.AvailableSeats -= 2;
        await contextA.SaveChangesAsync();

        // Second writer's xmin is now stale.
        rideB.AvailableSeats -= 2;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextB.SaveChangesAsync());

        // The database reflects only the first write — never -1.
        await using var verify = _database.NewContext();
        var final = await verify.Rides.SingleAsync(r => r.Id == SeededRideId);
        Assert.Equal(1, final.AvailableSeats);
    }

    [SkippableFact]
    public async Task CarpoolUnitOfWork_TranslatesTheConflict_ToConcurrencyConflictException()
    {
        Skip.IfNot(_database.Available, $"PostgreSQL not available: {_database.UnavailableReason}");

        await ResetSeatsAsync(2);

        await using var contextA = _database.NewContext();
        await using var contextB = _database.NewContext();
        var unitOfWorkB = new CarpoolUnitOfWork(contextB);

        var rideA = await contextA.Rides.SingleAsync(r => r.Id == SeededRideId);
        var rideB = await contextB.Rides.SingleAsync(r => r.Id == SeededRideId);

        rideA.AvailableSeats -= 1;
        await contextA.SaveChangesAsync();

        rideB.AvailableSeats -= 1;

        // The Data layer maps EF's DbUpdateConcurrencyException to the domain exception the
        // API middleware turns into HTTP 409 (spec §23, §24).
        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => unitOfWorkB.SaveChangesAsync(CancellationToken.None));
    }
}
