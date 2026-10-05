namespace Carpool.Core.Configuration;

/// <summary>
/// JWT settings bound from the <c>"Jwt"</c> configuration section (spec §40, §67).
///
/// <see cref="Issuer"/>, <see cref="Audience"/> and <see cref="ExpiryMinutes"/> come from
/// <c>appsettings.json</c>. <see cref="Key"/> is the HMAC signing secret and is supplied
/// only through User Secrets (Development) or environment variables — never committed
/// (spec §67).
///
/// Lives in Core so both <c>Carpool.Service</c> (the token generator) and
/// <c>Carpool.API</c> (the bearer-validation pipeline in <c>Program.cs</c>) bind to the
/// same shape.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>HMAC-SHA256 signing key. Secret — never stored in <c>appsettings.json</c>.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
