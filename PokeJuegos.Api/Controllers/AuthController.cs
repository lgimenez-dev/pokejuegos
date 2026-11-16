using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PokeJuegos.Api.Helpers;
using PokeJuegos.Api.Models;
using PokeJuegos.Application;
using PokeJuegos.Application.Auth;
using PokeJuegos.Application.Services;
using PokeJuegos.Domain;
using PokeJuegos.DTO;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace PokeJuegos.Api.Controllers;

/// <summary>
/// Handles authentication and account-lifecycle endpoints under <c>/api/auth</c>.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IJwtService jwtService,
    IValidator<LoginRequest> loginValidator,
    IValidator<RegisterRequest> registerValidator,
    IValidator<RecoverPasswordRequest> recoverPasswordValidator,
    IValidator<ChangePasswordRequest> changePasswordValidator,
    IValidator<ActivateAccountRequest> activateAccountValidator,
    IValidator<CompleteProfileRequest> completeProfileValidator,
    IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Creates a new user account and sends an activation email if SMTP is configured.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK on success; 400 Bad Request on validation or business-rule failure.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        ValidationResult validation = await registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorResponse.From(validation));
        }

        Result<bool> result = await authService.RegisterAsync(request, ct);
        if (!result.IsSuccess)
        {
            return BadRequest(new BusinessErrorResponse(result.Error!));
        }

        return Ok();
    }

    /// <summary>
    /// Authenticates a user, issues a JWT, and sets it as an <c>HttpOnly</c> cookie named <c>access_token</c>.
    /// Limited to 5 requests per minute per IP.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK with <see cref="LoginResponse"/> on success; 400 on validation failure; 401 on auth failure.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        ValidationResult validation = await loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorResponse.From(validation));
        }

        Result<LoginResponse> result = await authService.LoginAsync(request, ct);
        if (!result.IsSuccess)
        {
            return Unauthorized(new BusinessErrorResponse(result.Error!));
        }

        Response.Cookies.Append("access_token", result.Value!.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(jwtService.ExpirationMinutes)
        });

        return Ok(result.Value);
    }

    /// <summary>
    /// Revokes the current session and deletes the <c>access_token</c> cookie.
    /// The token is read from the cookie first, falling back to the <c>Authorization</c> header.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK on success; 401 Unauthorized if the session is not found or already revoked.</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        string token = GetTokenFromRequest();
        Result<bool> result = await authService.LogoutAsync(token, ct);
        if (!result.IsSuccess)
        {
            return Unauthorized(new BusinessErrorResponse(result.Error!));
        }

        Response.Cookies.Delete("access_token");
        return Ok();
    }

    /// <summary>
    /// Confirms that the current JWT corresponds to an active, non-revoked, non-expired session.
    /// The token is read from the cookie first, falling back to the <c>Authorization</c> header.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK with <see cref="ValidateTokenResponse"/> on success; 401 Unauthorized if invalid.</returns>
    [HttpGet("validate-token")]
    [Authorize]
    public async Task<IActionResult> ValidateToken(CancellationToken ct)
    {
        string token = GetTokenFromRequest();
        Result<ValidateTokenResponse> result = await authService.ValidateTokenAsync(token, ct);
        if (!result.IsSuccess)
        {
            return Unauthorized(new BusinessErrorResponse(result.Error!));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the identity of the currently authenticated user from JWT claims.
    /// </summary>
    /// <returns>200 OK with <see cref="MeResponse"/> containing the user's id, name, email, and role.</returns>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        string userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        string name = User.FindFirstValue(JwtRegisteredClaimNames.Name)!;
        string email = User.FindFirstValue(JwtRegisteredClaimNames.Email)!;
        string role = User.FindFirstValue("role")!;

        return Ok(new MeResponse(userId, name, email, role));
    }

    /// <summary>
    /// Initiates a password-recovery flow. Always returns 200 OK to prevent user enumeration.
    /// Sends a recovery email if the account exists and is not pending activation; OAuth2 accounts
    /// without a password get a link to create one. Limited to 3 requests per minute per IP.
    /// </summary>
    /// <param name="request">The email used to identify the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK always; 400 Bad Request on validation failure; 429 when rate limited.</returns>
    [HttpPost("recover-password")]
    [EnableRateLimiting("recover-password")]
    public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordRequest request, CancellationToken ct)
    {
        ValidationResult validation = await recoverPasswordValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorResponse.From(validation));
        }

        await authService.RecoverPasswordAsync(request, ct);
        return Ok();
    }

    /// <summary>
    /// Activates a pending account using the one-time token sent in the registration email.
    /// The password chosen at registration is kept.
    /// </summary>
    /// <param name="request">The activation token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK on success; 400 Bad Request on validation failure or an invalid/expired token.</returns>
    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateAccountRequest request, CancellationToken ct)
    {
        ValidationResult validation = await activateAccountValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorResponse.From(validation));
        }

        Result<bool> result = await authService.ActivateAsync(request, ct);
        if (!result.IsSuccess)
        {
            return BadRequest(new BusinessErrorResponse(result.Error!));
        }

        return Ok();
    }

    /// <summary>
    /// Sets a new password using a one-time token received by email (password-recovery flow).
    /// </summary>
    /// <param name="request">The one-time token and the new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK on success; 400 Bad Request on validation or invalid/expired token.</returns>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        ValidationResult validation = await changePasswordValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorResponse.From(validation));
        }

        Result<bool> result = await authService.ChangePasswordAsync(request, ct);
        if (!result.IsSuccess)
        {
            return BadRequest(new BusinessErrorResponse(result.Error!));
        }

        return Ok();
    }

    /// <summary>
    /// Sets the username and name of the current user. Meant for OAuth2 users, who are created without a username.
    /// </summary>
    /// <param name="request">The username and name to set.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK on success; 400 Bad Request on validation failure, taken username, or an already complete profile.</returns>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileRequest request, CancellationToken ct)
    {
        ValidationResult validation = await completeProfileValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorResponse.From(validation));
        }

        Guid userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        Result<bool> result = await authService.CompleteProfileAsync(userId, request, ct);
        if (!result.IsSuccess)
        {
            return BadRequest(new BusinessErrorResponse(result.Error!));
        }

        return Ok();
    }

    /// <summary>
    /// Initiates the Google OAuth2 login flow by issuing a challenge that redirects the user to the
    /// Google consent screen. On success, the callback sets an <c>access_token</c> HttpOnly cookie
    /// and redirects to the configured frontend URL.
    /// Native clients pass <paramref name="returnUrl"/> (the app's custom scheme or a loopback listener, see
    /// <see cref="OAuthReturnUrl"/>) and get the token back there in the query string instead.
    /// </summary>
    /// <param name="returnUrl">Optional native-client redirect target.</param>
    /// <returns>A challenge result that triggers the Google OAuth2 redirect; 400 if <paramref name="returnUrl"/> is not allowed.</returns>
    [HttpGet("oauth2/google")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl)
    {
        AuthenticationProperties properties = new();
        if (returnUrl is not null)
        {
            if (!OAuthReturnUrl.IsAllowed(returnUrl, configuration))
            {
                return BadRequest(new BusinessErrorResponse(ErrorMessages.InvalidReturnUrl));
            }

            properties.Items[OAuthReturnUrl.ItemKey] = returnUrl;
        }

        return Challenge(properties, "Google");
    }

    /// <summary>
    /// Reads the JWT from the <c>access_token</c> cookie, falling back to the <c>Authorization: Bearer</c> header.
    /// </summary>
    /// <returns>The raw JWT string.</returns>
    private string GetTokenFromRequest() =>
        Request.Cookies["access_token"]
        ?? Request.Headers.Authorization.ToString().Replace("Bearer ", "");
}
