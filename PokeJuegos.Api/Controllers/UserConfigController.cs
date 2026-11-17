using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PokeJuegos.Api.Models;
using PokeJuegos.Application.UserConfigs;
using PokeJuegos.Domain;
using PokeJuegos.DTO;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PokeJuegos.Api.Controllers;

/// <summary>
/// Handles the current user's preferences under <c>/api/user-config</c>.
/// </summary>
[ApiController]
[Route("api/user-config")]
[Authorize]
public class UserConfigController(
    IUserConfigService userConfigService,
    IValidator<UserConfigRequest> userConfigValidator) : ControllerBase
{
    /// <summary>
    /// Returns the current user's preferences, or the defaults if they never saved any.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK with <see cref="UserConfigResponse"/> (<c>updatedAt</c> is null for the defaults).</returns>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        Result<UserConfigResponse> result = await userConfigService.GetAsync(CurrentUserId(), ct);
        return Ok(result.Value);
    }

    /// <summary>
    /// Syncs the client's copy of the preferences: it is saved only if it is newer than the server's (last write wins).
    /// </summary>
    /// <param name="request">The whole configuration and when the client last changed it.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 OK with <see cref="UserConfigResponse"/>: what the server holds after the call. If its <c>updatedAt</c> is newer
    /// than the client's, the server copy won and the client should apply it. 400 Bad Request on validation failure.
    /// </returns>
    [HttpPut]
    public async Task<IActionResult> Sync([FromBody] UserConfigRequest request, CancellationToken ct)
    {
        ValidationResult validation = await userConfigValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return BadRequest(ValidationErrorResponse.From(validation));
        }

        Result<UserConfigResponse> result = await userConfigService.SyncAsync(CurrentUserId(), request, ct);
        return Ok(result.Value);
    }

    /// <summary>
    /// Reads the current user's ID from the JWT <c>sub</c> claim.
    /// </summary>
    /// <returns>The user's ID.</returns>
    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
