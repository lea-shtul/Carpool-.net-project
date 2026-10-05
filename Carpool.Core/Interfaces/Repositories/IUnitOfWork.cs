namespace Carpool.Core.Interfaces.Repositories;

/// <summary>
/// Commits the work tracked by the repositories and provides an atomic transaction
/// boundary for multi-record operations (spec §68): booking (seat decrement + insert),
/// booking cancellation (status + seat return), ride cancellation/completion
/// (ride status + cascade to bookings).
///
/// Implemented in <c>Carpool.Data</c> over the EF Core <c>DbContext</c>. The
/// implementation is responsible for catching EF Core's
/// <c>DbUpdateConcurrencyException</c> and surfacing it as
/// <see cref="Exceptions.ConcurrencyConflictException"/> so the Service layer stays
/// free of an EF Core dependency (spec §5, §23).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Runs <paramref name="operation"/> inside a database transaction, committing on
    /// success and rolling back on any exception.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
