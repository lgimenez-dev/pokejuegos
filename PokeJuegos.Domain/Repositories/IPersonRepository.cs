using PokeJuegos.Domain.Entities;

namespace PokeJuegos.Domain.Repositories;

/// <summary>
/// Defines read and write operations for the <see cref="Person"/> entity.
/// </summary>
public interface IPersonRepository
{
    /// <summary>
    /// Returns the profile linked to the specified user, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Person?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns the profile with the specified national identity document number, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="nationalId">The national identity document number to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Person?> GetByNationalIdAsync(string nationalId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new person profile to the repository. Changes are persisted via <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </summary>
    /// <param name="person">The person entity to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(Person person, CancellationToken ct = default);
}
