using PokeJuegos.Domain;
using PokeJuegos.DTO;

namespace PokeJuegos.Application.Sessions;

/// <summary>
/// Defines operations for listing and revoking user sessions.
/// </summary>
public interface IUserSessionService
{
    /// <summary>
    /// Returns all active (non-revoked, non-expired) sessions for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful result with the list of sessions, or a failure if the user does not exist.
    /// </returns>
    Task<Result<IReadOnlyList<UserSessionResponse>>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Revokes all active sessions for the specified user and returns the revoked count.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful result with the number of revoked sessions, or a failure if the user does not exist.
    /// </returns>
    Task<Result<RevokeSessionsResponse>> RevokeSessionsByUserIdAsync(Guid userId, CancellationToken ct = default);
}
