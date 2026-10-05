using Carpool.Core.Configuration;
using Xunit;

namespace Carpool.Tests.Configuration;

/// <summary>
/// Tests for <see cref="DatabaseUrlConverter"/> — the DATABASE_URL to Npgsql
/// connection-string conversion used by the Docker deployment (not part of the base
/// spec; see Carpool.API/Program.cs). Covers the Render production incident this guards
/// against: Render's Internal Database URL omits the port, which previously produced an
/// invalid "Port=-1" connection string and crashed the app on startup.
/// </summary>
public class DatabaseUrlConverterTests
{
    [Fact]
    public void ToNpgsqlConnectionString_WithExplicitPort_UsesIt()
    {
        var result = DatabaseUrlConverter.ToNpgsqlConnectionString("postgres://user:pass@example-host:5433/mydb");

        Assert.Contains("Host=example-host", result);
        Assert.Contains("Port=5433", result);
        Assert.Contains("Database=mydb", result);
        Assert.Contains("Username=user", result);
        Assert.Contains("Password=pass", result);
    }

    [Fact]
    public void ToNpgsqlConnectionString_WithoutPort_DefaultsTo5432()
    {
        // Render's Internal Database URL has exactly this shape — no port — which is the
        // case that previously crashed the app (Uri.Port == -1 reached Npgsql verbatim).
        var result = DatabaseUrlConverter.ToNpgsqlConnectionString("postgres://user:pass@dpg-xxxx-a/carpool_db");

        Assert.Contains("Host=dpg-xxxx-a", result);
        Assert.Contains("Port=5432", result);
        Assert.Contains("Database=carpool_db", result);
        Assert.DoesNotContain("Port=-1", result);
    }

    [Fact]
    public void ToNpgsqlConnectionString_UsesPreferSslMode_NotRequire()
    {
        // Require would refuse Render's internal (non-TLS) network connections outright.
        var result = DatabaseUrlConverter.ToNpgsqlConnectionString("postgres://user:pass@dpg-xxxx-a/carpool_db");

        Assert.Contains("SSL Mode=Prefer", result);
        Assert.DoesNotContain("SSL Mode=Require", result);
    }

    [Fact]
    public void ToNpgsqlConnectionString_UrlEncodedCredentials_AreDecoded()
    {
        var result = DatabaseUrlConverter.ToNpgsqlConnectionString("postgres://user:p%40ss%3Aw0rd@dpg-xxxx-a/db");

        Assert.Contains("Password=p@ss:w0rd", result);
    }

    [Fact]
    public void ToNpgsqlConnectionString_NoPassword_ResultsInEmptyPassword()
    {
        var result = DatabaseUrlConverter.ToNpgsqlConnectionString("postgres://user@dpg-xxxx-a/db");

        Assert.Contains("Username=user", result);
        Assert.Contains("Password=;", result);
    }
}
