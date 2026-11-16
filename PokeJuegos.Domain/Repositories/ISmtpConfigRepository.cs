using PokeJuegos.Domain.Entities;

namespace PokeJuegos.Domain.Repositories;

/// <summary>
/// Defines read and write operations for the <see cref="SmtpConfig"/> entity.
/// </summary>
public interface ISmtpConfigRepository
{
    /// <summary>
    /// Returns the current SMTP configuration, or <see langword="null"/> if none has been created yet.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<SmtpConfig?> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds the SMTP configuration to the repository. Changes are persisted via <c>IUnitOfWork.SaveChangesAsync</c>.
    /// </summary>
    /// <param name="smtpConfig">The SMTP configuration entity to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(SmtpConfig smtpConfig, CancellationToken ct = default);
}
