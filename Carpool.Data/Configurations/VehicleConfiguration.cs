using Carpool.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carpool.Data.Configurations;

/// <summary>
/// Fluent API mapping for <see cref="Vehicle"/> (spec §8, §45). Auto-discovered by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="CarpoolDbContext.OnModelCreating"/>.
///
/// Business rules enforced at the database level here:
/// <list type="bullet">
///   <item><description>LicensePlate is unique (spec §8, §45).</description></item>
///   <item><description>PassengerCapacity must be positive (spec §8) — a check constraint
///   backs up the Service-layer validation.</description></item>
///   <item><description>Every vehicle belongs to exactly one <see cref="User"/> and a user
///   can own many (spec §8, §32). The owner FK is non-nullable and uses
///   <see cref="DeleteBehavior.Restrict"/> so a user with vehicles cannot be deleted and
///   historical Ride → Vehicle links are preserved (spec §9, §73).</description></item>
/// </list>
///
/// The <c>Vehicle</c> → <c>Rides</c> one-to-many is configured from the dependent side in
/// <c>RideConfiguration</c>.
/// </summary>
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Manufacturer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.LicensePlate)
            .IsRequired()
            .HasMaxLength(20);

        // Unique index on LicensePlate (spec §8, §45).
        builder.HasIndex(v => v.LicensePlate)
            .IsUnique();

        builder.Property(v => v.PassengerCapacity)
            .IsRequired();

        // Defense in depth behind the Service-layer rule (spec §8).
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Vehicle_PassengerCapacity_Positive",
            "\"PassengerCapacity\" > 0"));

        // Vehicle → User (owner). One user owns many vehicles (spec §8, §32).
        builder.HasOne(v => v.Owner)
            .WithMany(u => u.Vehicles)
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
