using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace PokeJuegos.Api.Extensions;

/// <summary>
/// Extension methods for resolving the language of each request (English or Spanish).
/// </summary>
public static class LocalizationExtensions
{
    private static readonly CultureInfo[] SupportedCultures = [new("en"), new("es")];

    /// <summary>
    /// Configures request localization: the culture comes from the <c>Accept-Language</c> header, limited to
    /// <c>en</c> and <c>es</c>, and falls back to <c>en</c>. Used to pick the language of the emails.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddDefaultRequestLocalization(this IServiceCollection services)
    {
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en");
            options.SupportedCultures = SupportedCultures;
            options.SupportedUICultures = SupportedCultures;
            options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
        });

        return services;
    }
}
