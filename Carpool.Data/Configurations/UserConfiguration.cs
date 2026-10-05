using Carpool.Core.Entities;
using Carpool.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carpool.Data.Configurations;

/// <summary>
/// Fluent API mapping for <see cref="User"/> (spec §7, §45). Auto-discovered by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="CarpoolDbContext.OnModelCreating"/>.
///
/// Business rules enforced at the database level here:
/// <list type="bullet">
///   <item><description>Email is unique (spec §7, §45) — a unique index, so duplicate
///   registrations fail even under concurrent requests.</description></item>
///   <item><description>The password column only ever holds a hash (spec §7, §39). The
///   entity has no plaintext field; this configuration just makes the hash column required.</description></item>
/// </list>
///
/// The five relationships that originate from <see cref="User"/> (Vehicles, RidesAsDriver,
/// Bookings, RatingsGiven, RatingsReceived) are configured from the dependent side in
/// <c>VehicleConfiguration</c>, <c>RideConfiguration</c>, <c>BookingConfiguration</c> and
/// <c>RatingConfiguration</c>, where the foreign-key column and its <c>DeleteBehavior</c>
/// live next to the entity that owns them.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        // Unique index on Email (spec §7, §45).
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(30);

        // Store the enum as its name ("User" / "Admin") rather than an integer so
        // rows and migrations stay readable and are not tied to declaration order.
        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(UserRole.User);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
