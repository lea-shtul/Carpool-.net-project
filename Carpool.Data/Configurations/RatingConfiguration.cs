using Carpool.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carpool.Data.Configurations;

/// <summary>
/// Fluent API mapping for <see cref="Rating"/> (spec §27–§29, §45). Auto-discovered by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="CarpoolDbContext.OnModelCreating"/>.
///
/// Database-level rules configured here:
/// <list type="bullet">
///   <item><description>Exactly one rating per <c>(RideId, ReviewerId)</c> — the spec
///   explicitly requires this uniqueness at the database level so concurrent requests
///   cannot both insert (spec §28).</description></item>
///   <item><description><c>Score</c> is constrained to the 1–5 range by a check constraint
///   (spec §27), behind the authoritative Service-layer and request-DTO validation.</description></item>
///   <item><description><see cref="Rating"/> has two foreign keys to <see cref="User"/>
///   (<see cref="Rating.ReviewerId"/> and <see cref="Rating.DriverId"/>); EF cannot pair
///   them with the <c>RatingsGiven</c> / <c>RatingsReceived</c> navigations automatically,
///   so both are configured explicitly here. All three foreign keys use
///   <see cref="DeleteBehavior.Restrict"/> so rating history is preserved.</description></item>
/// </list>
/// </summary>
public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Score)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Defense in depth behind the Service-layer / request-DTO validation (spec §27).
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Rating_Score_Range",
            "\"Score\" >= 1 AND \"Score\" <= 5"));

        // One rating per ride per reviewer — required at the DB level by spec §28.
        builder.HasIndex(r => new { r.RideId, r.ReviewerId })
            .IsUnique();

        // Rating → Ride. One completed ride can have many ratings (one per passenger).
        builder.HasOne(r => r.Ride)
            .WithMany(x => x.Ratings)
            .HasForeignKey(r => r.RideId)
            .OnDelete(DeleteBehavior.Restrict);

        // Rating → User (reviewer / passenger who wrote it).
        builder.HasOne(r => r.Reviewer)
            .WithMany(u => u.RatingsGiven)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Rating → User (driver being rated). Denormalised from the ride for easy lookup
        // of a driver's ratings via GET /api/users/{userId}/ratings (spec §29).
        builder.HasOne(r => r.Driver)
            .WithMany(u => u.RatingsReceived)
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
