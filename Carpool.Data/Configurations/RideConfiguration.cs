using Carpool.Core.Entities;
using Carpool.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carpool.Data.Configurations;

/// <summary>
/// Fluent API mapping for <see cref="Ride"/> (spec §10, §11, §23, §45, §46). Auto-discovered
/// by <c>ApplyConfigurationsFromAssembly</c> in <see cref="CarpoolDbContext.OnModelCreating"/>.
///
/// The ride is the limited resource at the centre of the concurrency requirement. The key
/// database-level rules configured here:
/// <list type="bullet">
///   <item><description><see cref="Ride.RowVersion"/> is mapped to PostgreSQL's <c>xmin</c>
///   system column (type <c>xid</c>) as an optimistic concurrency token (spec §23, §46).
///   No extra column is added; Npgsql compares <c>xmin</c> in the <c>UPDATE ... WHERE</c>
///   clause, so two requests that both read the same ride cannot both save a seat change —
///   the second <c>SaveChangesAsync</c> throws <c>DbUpdateConcurrencyException</c>, which the
///   Service layer turns into 409. This deliberately avoids SQL Server <c>rowversion</c> /
///   <c>[Timestamp]</c> / <c>byte[]</c>.</description></item>
///   <item><description>Check constraints back up the seat invariants from spec §11:
///   <c>TotalSeats &gt; 0</c> and <c>0 &lt;= AvailableSeats &lt;= TotalSeats</c>. The
///   authoritative enforcement is still in the Service layer.</description></item>
///   <item><description>The <c>Driver</c> and <c>Vehicle</c> foreign keys are non-nullable
///   and use <see cref="DeleteBehavior.Restrict"/> so a ride's history — including which
///   vehicle it used (spec §9) — is always preserved.</description></item>
/// </list>
///
/// There is intentionally no update endpoint for a ride (spec §13); this configuration does
/// not need to account for edits, only for the dedicated cancel/complete state transitions.
/// The <c>Bookings</c>, <c>RideTags</c> and <c>Ratings</c> collections are configured from
/// their own dependent-side files.
/// </summary>
public class RideConfiguration : IEntityTypeConfiguration<Ride>
{
    public void Configure(EntityTypeBuilder<Ride> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Origin)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Destination)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.DepartureTime)
            .IsRequired();

        builder.Property(r => r.TotalSeats)
            .IsRequired();

        builder.Property(r => r.AvailableSeats)
            .IsRequired();

        builder.Property(r => r.PricePerSeat)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(r => r.EstimatedDurationMinutes)
            .IsRequired();

        // Store the enum as its name ("Scheduled" / "InProgress" / "Completed" / "Cancelled")
        // rather than an integer so rows and migrations stay readable (spec §10).
        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(RideStatus.Scheduled);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // StartedAt / CompletedAt are nullable by CLR type; no configuration needed.

        // Optimistic concurrency token mapped to PostgreSQL's xmin system column (spec §23, §46).
        builder.Property(r => r.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // Seat invariants (spec §11) — defense in depth behind the Service-layer rules.
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Ride_TotalSeats_Positive",
                "\"TotalSeats\" > 0");
            t.HasCheckConstraint(
                "CK_Ride_AvailableSeats_Range",
                "\"AvailableSeats\" >= 0 AND \"AvailableSeats\" <= \"TotalSeats\"");
        });

        // Ride → User (driver). One user drives many rides (spec §10, §32).
        builder.HasOne(r => r.Driver)
            .WithMany(u => u.RidesAsDriver)
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ride → Vehicle. A vehicle is used by many rides over time and cannot be deleted
        // while any ride references it (spec §9, §32).
        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.Rides)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
