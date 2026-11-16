using PokeJuegos.Api.Models;
using PokeJuegos.Application.Sessions;
using PokeJuegos.Domain;
using PokeJuegos.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PokeJuegos.Api.Controllers;

/// <summary>
/// Handles session listing and revocation endpoints under <c>/api/sessions</c>.
/// All endpoints require a valid JWT.
/// </summary>
[ApiController]
[Route("api/sessions")]
[Authorize]
public class UserSessionController(IUserSessionService userSessionService) : ControllerBase
{
    /// <summary>
    /// Returns all active (non-revoked, non-expired) sessions for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK with the list of active sessions; 404 Not Found if the user does not exist.</returns>
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetActiveSessions([FromRoute] Guid userId, CancellationToken ct)
    {
        Result<IReadOnlyList<UserSessionResponse>> result = await userSessionService.GetActiveSessionsByUserIdAsync(userId, ct);

        if (!result.IsSuccess)
        {
            return NotFound(new BusinessErrorResponse(result.Error!));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Revokes all active sessions for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 OK with the number of revoked sessions; 404 Not Found if the user does not exist.</returns>
    [HttpPost("user/{userId:guid}/revoke")]
    public async Task<IActionResult> RevokeSessions([FromRoute] Guid userId, CancellationToken ct)
    {
        Result<RevokeSessionsResponse> result = await userSessionService.RevokeSessionsByUserIdAsync(userId, ct);

        if (!result.IsSuccess)
        {
            return NotFound(new BusinessErrorResponse(result.Error!));
        }

        return Ok(result.Value);
    }
}
