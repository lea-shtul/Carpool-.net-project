using Carpool.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Carpool.Data;

/// <summary>
/// EF Core Code First database context for the Carpool API (spec §44).
/// Holds one <see cref="DbSet{TEntity}"/> per domain entity. The PostgreSQL
/// provider and connection string are supplied through <see cref="DbContextOptions"/>
/// at registration time in <c>Program.cs</c> — never hard-coded here.
///
/// Keys, foreign keys, unique indexes, the composite key on <see cref="RideTag"/>,
/// the many-to-many between <see cref="Ride"/> and <see cref="Tag"/> and the
/// PostgreSQL <c>xmin</c> concurrency token are configured in Stage 4 via
/// <see cref="Microsoft.EntityFrameworkCore.IEntityTypeConfiguration{TEntity}"/>
/// classes, not in this file.
/// </summary>
public class CarpoolDbContext : DbContext
{
    public CarpoolDbContext(DbContextOptions<CarpoolDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Ride> Rides => Set<Ride>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<Rating> Ratings => Set<Rating>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<RideTag> RideTags => Set<RideTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Auto-discovers every IEntityTypeConfiguration<T> in this assembly
        // (Carpool.Data/Configurations/*.cs) so each entity's Fluent API mapping
        // lives in its own file rather than one large OnModelCreating (spec §45).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarpoolDbContext).Assembly);

        // Deterministic HasData seed (spec §49), emitted as the separate "SeedData" migration.
        Seed.SeedData.Seed(modelBuilder);
    }
}
