using Carpool.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Carpool.Data.Configurations;

/// <summary>
/// Fluent API mapping for <see cref="RideTag"/>, the join entity that implements the
/// many-to-many relationship between <see cref="Ride"/> and <see cref="Tag"/>
/// (spec §31, §32, §45). Auto-discovered by <c>ApplyConfigurationsFromAssembly</c> in
/// <see cref="CarpoolDbContext.OnModelCreating"/>.
///
/// Database-level rules configured here:
/// <list type="bullet">
///   <item><description>Composite primary key <c>(RideId, TagId)</c> (spec §31). This also
///   provides the unique <c>(RideId, TagId)</c> constraint required by spec §45, so no
///   separate index is needed.</description></item>
///   <item><description>Two foreign keys — to <see cref="Ride"/> and to <see cref="Tag"/> —
///   each with <see cref="DeleteBehavior.Cascade"/>. Join rows are plain links, not
///   historical records, so removing a ride (or a tag) removes its associations. In
///   practice rides and tags are never deleted.</description></item>
/// </list>
/// </summary>
public class RideTagConfiguration : IEntityTypeConfiguration<RideTag>
{
    public void Configure(EntityTypeBuilder<RideTag> builder)
    {
        // Composite primary key (spec §31).
        builder.HasKey(rt => new { rt.RideId, rt.TagId });

        // RideTag → Ride.
        builder.HasOne(rt => rt.Ride)
            .WithMany(r => r.RideTags)
            .HasForeignKey(rt => rt.RideId)
            .OnDelete(DeleteBehavior.Cascade);

        // RideTag → Tag.
        builder.HasOne(rt => rt.Tag)
            .WithMany(t => t.RideTags)
            .HasForeignKey(rt => rt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
