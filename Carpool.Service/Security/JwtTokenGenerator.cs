using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Carpool.Core.Configuration;
using Carpool.Core.Entities;
using Carpool.Core.Interfaces.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Carpool.Service.Security;

/// <summary>
/// Implements <see cref="IJwtTokenGenerator"/> (spec §40). Produces an HMAC-SHA256 signed
/// JWT carrying the user id (<c>sub</c>), <c>email</c> and <c>role</c> claims plus a unique
/// <c>jti</c>. Issuer, audience, signing key and lifetime all come from
/// <see cref="JwtOptions"/> — never hard-coded (spec §67). The role is taken from the
/// persisted <see cref="User"/>, never from client input (spec §40).
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public GeneratedToken Generate(User user)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(_options.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new GeneratedToken(accessToken, expiresAtUtc);
    }
}
