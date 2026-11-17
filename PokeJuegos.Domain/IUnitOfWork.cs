using PokeJuegos.Domain.Repositories;

namespace PokeJuegos.Domain;

/// <summary>
/// Coordinates changes across all repositories and persists them as a single atomic operation.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Gets the user repository.</summary>
    IUserRepository Users { get; }

    /// <summary>Gets the person repository.</summary>
    IPersonRepository Persons { get; }

    /// <summary>Gets the user session repository.</summary>
    IUserSessionRepository UserSessions { get; }

    /// <summary>Gets the SMTP configuration repository.</summary>
    ISmtpConfigRepository SmtpConfigs { get; }

    /// <summary>Gets the user configuration repository.</summary>
    IUserConfigRepository UserConfigs { get; }

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
