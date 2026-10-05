using Carpool.Core.Entities;
using Carpool.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carpool.Data.Configurations;

/// <summary>
/// Fluent API mapping for <see cref="Booking"/> (spec §21, §22, §26, §45). Auto-discovered
/// by <c>ApplyConfigurationsFromAssembly</c> in <see cref="CarpoolDbContext.OnModelCreating"/>.
///
/// Database-level rules configured here:
/// <list type="bullet">
///   <item><description>A passenger may hold at most one <see cref="BookingStatus.Active"/>
///   booking per ride (spec §26). A <b>filtered</b> unique index on
///   <c>(RideId, PassengerId)</c> restricted to <c>Status = 'Active'</c> enforces this even
///   under concurrent requests, while Cancelled / Completed rows do not block a new
///   booking.</description></item>
///   <item><description><c>NumberOfSeats &gt; 0</c> (spec §22) as a check constraint behind
///   the Service-layer validation.</description></item>
///   <item><description>Booking rows are never deleted — cancelling only changes
///   <see cref="Booking.Status"/> (spec §21, §25). Both foreign keys use
///   <see cref="DeleteBehavior.Restrict"/> so historical booking records are always
///   preserved (spec §73).</description></item>
/// </list>
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.NumberOfSeats)
            .IsRequired();

        // Defense in depth behind the Service-layer rule (spec §22).
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Booking_NumberOfSeats_Positive",
            "\"NumberOfSeats\" > 0"));

        // Store the enum as its name ("Active" / "Cancelled" / "Completed") rather than an
        // integer so rows and migrations stay readable (spec §21).
        builder.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(BookingStatus.Active);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        // CancelledAt is nullable by CLR type; no configuration needed.

        // At most one Active booking per (ride, passenger) — enforced at the DB level so a
        // concurrent double-submit cannot create two (spec §26). Cancelled / Completed rows
        // are excluded by the filter, so re-booking after a cancellation is allowed.
        builder.HasIndex(b => new { b.RideId, b.PassengerId })
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'");

        // Booking → Ride. One ride has many bookings (spec §21, §32).
        builder.HasOne(b => b.Ride)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RideId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking → User (passenger). One user has many bookings (spec §21, §32).
        builder.HasOne(b => b.Passenger)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
