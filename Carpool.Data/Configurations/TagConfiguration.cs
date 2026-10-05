using Carpool.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carpool.Data.Configurations;

/// <summary>
/// Fluent API mapping for <see cref="Tag"/> (spec §30, §45). Auto-discovered by
/// <c>ApplyConfigurationsFromAssembly</c> in <see cref="CarpoolDbContext.OnModelCreating"/>.
///
/// Beyond the base spec, this project extends tags with a global/private split (see
/// <see cref="Tag"/>): <see cref="Tag.OwnerId"/> is a nullable FK to <see cref="User"/>, and
/// <see cref="Tag.Name"/> is unique within its scope rather than globally —
/// <list type="bullet">
///   <item><description>a filtered unique index on <c>Name</c> where <c>OwnerId IS NULL</c>
///   keeps global tag names unique among themselves;</description></item>
///   <item><description>a filtered unique index on <c>(OwnerId, Name)</c> where
///   <c>OwnerId IS NOT NULL</c> keeps each user's private tag names unique within their own
///   set, independent of every other user's and of the global names.</description></item>
/// </list>
/// A user is never hard-deleted while they still own private tags — <c>Restrict</c> on the
/// owner FK, matching every other FK in this project (spec §73 — historical data preserved).
///
/// The join side of the Ride ↔ Tag many-to-many is configured in <c>RideTagConfiguration</c>.
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasFilter("\"OwnerId\" IS NULL");

        builder.HasIndex(t => new { t.OwnerId, t.Name })
            .IsUnique()
            .HasFilter("\"OwnerId\" IS NOT NULL");

        // Tag → User (owner). Null for a global/seed tag (spec extension — see Tag).
        builder.HasOne(t => t.Owner)
            .WithMany()
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
