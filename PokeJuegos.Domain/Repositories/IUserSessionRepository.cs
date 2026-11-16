using PokeJuegos.Domain.Entities;

namespace PokeJuegos.Domain.Repositories;

/// <summary>
/// Defines read and write operations for the <see cref="UserSession"/> entity.
/// </summary>
public interface IUserSessionRepository
{
    /// <summary>
    /// Returns the session whose token hash matches the specified value, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="tokenHash">The HMAC-SHA256 token hash to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<UserSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// Returns all non-revoked, non-expired sessions for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<UserSession>> ListActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new session to the repository. Changes are persisted via <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </summary>
    /// <param name="session">The session entity to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(UserSession session, CancellationToken ct = default);
}
