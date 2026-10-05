using Carpool.Core.Exceptions;
using Carpool.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Carpool.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> (spec §52, §68). Registered Scoped
/// alongside <see cref="CarpoolDbContext"/>.
///
/// This is the single place where:
/// <list type="bullet">
///   <item><description>tracked changes staged by the repositories are committed
///   (<see cref="SaveChangesAsync"/>);</description></item>
///   <item><description>a multi-row operation runs inside one database transaction
///   (<see cref="ExecuteInTransactionAsync"/>) — booking (seat decrement + insert), booking
///   cancellation, ride cancellation/completion with its cascade to bookings (spec §14, §18,
///   §25, §68);</description></item>
///   <item><description>EF Core's <see cref="DbUpdateConcurrencyException"/> is translated to
///   <see cref="ConcurrencyConflictException"/>, so the Service layer never references EF
///   Core and the API middleware maps the conflict to <c>409</c> rather than <c>500</c>
///   (spec §5, §23, §24).</description></item>
/// </list>
/// </summary>
public class CarpoolUnitOfWork : IUnitOfWork
{
    private readonly CarpoolDbContext _context;

    public CarpoolUnitOfWork(CarpoolDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // A concurrent request changed a row (e.g. Ride.AvailableSeats) between our
            // read and this save; the xmin token no longer matches (spec §23). Rethrow as
            // a domain exception so the Service layer stays free of an EF Core dependency
            // and the middleware maps it to 409, not 500 (spec §5, §24).
            throw new ConcurrencyConflictException();
        }
    }
    //טרנזקציה של הזמנת נסיעה- עדכון מקומות שנשארו& שמירת הזמנה
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            // Any failure — a business-rule exception, a concurrency conflict already
            // translated by SaveChangesAsync, or an infrastructure error — rolls the whole
            // operation back so partial writes never persist (spec §68).
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
