using Carpool.Data;
using Microsoft.EntityFrameworkCore;

namespace Carpool.Tests.TestKit;

/// <summary>
/// A throwaway PostgreSQL database (default <c>carpool_test</c>, or the
/// <c>CARPOOL_TEST_DB</c> connection string) used by the optimistic-concurrency test
/// (spec §69). Created and migrated once for the collection, dropped afterwards; it never
/// touches the development <c>carpool</c> database.
///
/// If PostgreSQL is not reachable the fixture records that and the tests skip, so the suite
/// still passes on a machine without a database.
/// </summary>
public sealed class PostgresTestDatabase : IAsyncLifetime
{
    private readonly string _connectionString =
        Environment.GetEnvironmentVariable("CARPOOL_TEST_DB")
        ?? "Host=localhost;Port=5432;Database=carpool_test;Username=postgres;Password=postgres";

    public bool Available { get; private set; }

    public string? UnavailableReason { get; private set; }

    public CarpoolDbContext NewContext() =>
        new(new DbContextOptionsBuilder<CarpoolDbContext>().UseNpgsql(_connectionString).Options);

    public async Task InitializeAsync()
    {
        try
        {
            await using var db = NewContext();
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
            Available = true;
        }
        catch (Exception ex)
        {
            Available = false;
            UnavailableReason = ex.Message;
        }
    }

    public async Task DisposeAsync()
    {
        if (!Available)
        {
            return;
        }

        await using var db = NewContext();
        await db.Database.EnsureDeletedAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresIntegrationCollection : ICollectionFixture<PostgresTestDatabase>
{
    public const string Name = "PostgresIntegration";
}
