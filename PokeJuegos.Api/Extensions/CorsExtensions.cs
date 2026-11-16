using Microsoft.Extensions.DependencyInjection;

namespace PokeJuegos.Api.Extensions;

/// <summary>
/// Extension methods for configuring CORS policies.
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Registers a default CORS policy that allows any origin, header, and method.
    /// </summary>
    /// <param name="services">The service collection to add CORS to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddDefaultCors(this IServiceCollection services)
    {
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        return services;
    }
}
