using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PokeJuegos.Application.Services;
using PokeJuegos.Common.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace PokeJuegos.Infrastructure.Services;

/// <summary>
/// Generates signed JWTs using the secret and expiration configured in <c>appsettings.json</c>.
/// </summary>
internal sealed class JwtService : IJwtService
{
    private readonly string _secret;

    /// <inheritdoc/>
    public int ExpirationMinutes { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="JwtService"/>, reading
    /// <c>Jwt:Secret</c> and <c>Jwt:ExpirationMinutes</c> from <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>Jwt:Secret</c> or <c>Jwt:ExpirationMinutes</c> is missing from configuration.
    /// </exception>
    public JwtService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

        ExpirationMinutes = int.Parse(
            configuration["Jwt:ExpirationMinutes"]
            ?? throw new InvalidOperationException("Jwt:ExpirationMinutes is not configured."));
    }

    /// <inheritdoc/>
    public string GenerateToken(Guid userId, string name, string email, UserRole role)
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, name),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("role", role.ToString().ToUpperInvariant())
        ];

        JwtSecurityToken token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
