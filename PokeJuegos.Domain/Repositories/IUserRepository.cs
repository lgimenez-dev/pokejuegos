using PokeJuegos.Common.Enums;
using PokeJuegos.Domain.Entities;

namespace PokeJuegos.Domain.Repositories;

/// <summary>
/// Defines read and write operations for the <see cref="User"/> entity.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Returns the user with the specified ID, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns the user with the specified email address, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Returns the user with the specified username, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Returns the user whose active token matches the specified value, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="refreshToken">The token value to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// Returns the user whose provider and provider ID match the specified values, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="provider">The authentication provider to search for.</param>
    /// <param name="providerId">The external provider user ID to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<User?> GetByProviderAsync(UserProvider provider, string providerId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new user to the repository. Changes are persisted via <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(User user, CancellationToken ct = default);
}
