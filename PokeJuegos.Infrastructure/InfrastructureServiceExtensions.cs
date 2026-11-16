using PokeJuegos.Application.Services;
using PokeJuegos.Domain;
using PokeJuegos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PokeJuegos.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services into the DI container.
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers <see cref="AppDbContext"/> and all infrastructure services required by the application.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration used to resolve the connection string.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
