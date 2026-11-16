using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace PokeJuegos.Api.Extensions;

/// <summary>
/// Extension methods for configuring JWT Bearer authentication with HttpOnly cookie support.
/// </summary>
public static class JwtBearerExtensions
{
    /// <summary>
    /// Registers JWT Bearer authentication. Reads the token from the <c>access_token</c> HttpOnly cookie
    /// first, falling back to the <c>Authorization: Bearer</c> header.
    /// </summary>
    /// <param name="services">The service collection to add authentication to.</param>
    /// <param name="configuration">The application configuration used to resolve <c>Jwt:Secret</c>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddJwtBearerAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        byte[] jwtKey = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "role"
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        string? cookie = context.Request.Cookies["access_token"];
                        if (!string.IsNullOrEmpty(cookie))
                        {
                            context.Token = cookie;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
