using System.Security.Claims;
using PokeJuegos.Api.Helpers;
using PokeJuegos.Application.OAuth2;
using PokeJuegos.Application.Services;
using PokeJuegos.Domain;
using PokeJuegos.DTO;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PokeJuegos.Api.Extensions;

/// <summary>
/// Extension methods for configuring Google OAuth2 authentication.
/// </summary>
public static class GoogleAuthExtensions
{
    /// <summary>
    /// Registers Google OAuth2 authentication. On successful callback, issues a JWT <c>access_token</c>
    /// HttpOnly cookie and redirects to <c>App:FrontendUrl</c>. On failure, redirects with an
    /// <c>oauth_error</c> query parameter. Native clients that started the flow with a <c>returnUrl</c> are instead
    /// redirected there with <c>access_token</c> and <c>profile_complete</c> (or <c>error</c>) in the query string.
    /// </summary>
    /// <param name="services">The service collection to add authentication to.</param>
    /// <param name="configuration">The application configuration used to resolve <c>Google:ClientId</c>,
    /// <c>Google:ClientSecret</c>, and <c>App:FrontendUrl</c>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGoogleAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication()
            .AddCookie("External")
            .AddGoogle(options =>
            {
                options.SignInScheme = "External";
                options.ClientId = configuration["Google:ClientId"]!;
                options.ClientSecret = configuration["Google:ClientSecret"]!;
                options.CallbackPath = "/api/auth/oauth2/google/callback";
                options.Scope.Add("email");

                options.Events.OnTicketReceived = async ctx =>
                {
                    string? providerId = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                    string? email = ctx.Principal?.FindFirstValue(ClaimTypes.Email);
                    string? name = ctx.Principal?.FindFirstValue(ClaimTypes.Name);

                    IOAuth2Service oAuth2Service = ctx.HttpContext.RequestServices.GetRequiredService<IOAuth2Service>();
                    IJwtService jwtService = ctx.HttpContext.RequestServices.GetRequiredService<IJwtService>();
                    IConfiguration config = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

                    string frontendUrl = config["App:FrontendUrl"]!;

                    Result<LoginResponse> result = await oAuth2Service.HandleGoogleCallbackAsync(email!, name!, providerId!, ctx.HttpContext.RequestAborted);

                    ctx.HandleResponse();

                    // Native clients (validated in AuthController.GoogleLogin): hand the token back in the query
                    // string, not the fragment, because browsers never send the fragment to a loopback listener.
                    if (ctx.Properties?.Items.TryGetValue(OAuthReturnUrl.ItemKey, out string? returnUrl) == true && returnUrl is not null)
                    {
                        Dictionary<string, string?> query = result.IsSuccess
                            ? new()
                            {
                                ["access_token"] = result.Value!.Token,
                                ["profile_complete"] = result.Value.ProfileComplete ? "true" : "false",
                            }
                            : new() { ["error"] = result.Error };
                        ctx.HttpContext.Response.Redirect(QueryHelpers.AddQueryString(returnUrl, query));
                        return;
                    }

                    if (result.IsSuccess)
                    {
                        ctx.HttpContext.Response.Cookies.Append("access_token", result.Value!.Token,
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.None,
                                Expires = DateTimeOffset.UtcNow.AddMinutes(jwtService.ExpirationMinutes)
                            });
                        ctx.HttpContext.Response.Redirect(frontendUrl);
                    }
                    else
                    {
                        ctx.HttpContext.Response.Redirect($"{frontendUrl}?oauth_error={Uri.EscapeDataString(result.Error!)}");
                    }
                };
            });

        return services;
    }
}
