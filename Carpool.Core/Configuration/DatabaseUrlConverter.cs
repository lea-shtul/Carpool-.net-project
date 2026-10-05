namespace Carpool.Core.Configuration;

/// <summary>
/// Converts a cloud Postgres connection URI — e.g. Render's <c>DATABASE_URL</c>
/// (<c>postgres://user:pass@host[:port]/database</c>) — into the key=value connection
/// string Npgsql expects. Not part of the base spec; see <c>Carpool.API/Program.cs</c>,
/// which uses this only when <c>ConnectionStrings:CarpoolDb</c> is left empty (the
/// container deployment case). Lives in Core, with no EF Core / ASP.NET dependency, so it
/// is directly unit-testable (<c>Carpool.Tests/Configuration/DatabaseUrlConverterTests.cs</c>).
/// </summary>
public static class DatabaseUrlConverter
{
    public static string ToNpgsqlConnectionString(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');

        // Render's Internal Database URL omits the port entirely (it's always 5432 on
        // their internal network) — Uri.Port is -1 for a URI with no explicit port, which
        // is not a valid Npgsql port, so that case falls back to 5432.
        var port = uri.Port <= 0 ? 5432 : uri.Port;

        // SSL Mode=Prefer (not Require): Render's internal network connections may not be
        // TLS at all, while external ones are — Prefer negotiates TLS when available and
        // falls back to plaintext instead of refusing to connect outright. Trust Server
        // Certificate avoids shipping Render's CA bundle for what is, here, a student
        // deployment rather than a hardened production one.
        return $"Host={uri.Host};Port={port};Database={database};Username={username};" +
               $"Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
    }
}
