using PokeJuegos.Application.Auth;
using PokeJuegos.Application.OAuth2;
using PokeJuegos.Application.Sessions;
using PokeJuegos.Application.SmtpConfigs;
using PokeJuegos.Application.UserConfigs;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace PokeJuegos.Application;

/// <summary>
/// Extension methods for registering Application layer services into the DI container.
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Registers all FluentValidation validators and application services from the Application assembly.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddScoped<ISmtpConfigService, SmtpConfigService>();
        services.AddScoped<IOAuth2Service, OAuth2Service>();
        services.AddScoped<IUserConfigService, UserConfigService>();
        return services;
    }
}
