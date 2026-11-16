using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace PokeJuegos.Api.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI with JWT Bearer authentication.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Registers Swagger generation with a Bearer token security definition and global security requirement.
    /// </summary>
    /// <param name="services">The service collection to add Swagger to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });
            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }
}
