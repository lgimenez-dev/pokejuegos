using PokeJuegos.Domain.Entities;

namespace PokeJuegos.Domain.Repositories;

/// <summary>
/// Defines read and write operations for the <see cref="UserConfig"/> entity.
/// </summary>
public interface IUserConfigRepository
{
    /// <summary>
    /// Returns the preferences of the specified user, or <see langword="null"/> if they never saved any.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<UserConfig?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Adds the preferences of a user. Changes are persisted via <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </summary>
    /// <param name="config">The preferences to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(UserConfig config, CancellationToken ct = default);
}
