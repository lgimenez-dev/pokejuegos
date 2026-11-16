using PokeJuegos.Domain;
using PokeJuegos.DTO;

namespace PokeJuegos.Application.SmtpConfigs;

/// <summary>
/// Defines operations for managing the application's single SMTP configuration record.
/// </summary>
public interface ISmtpConfigService
{
    /// <summary>
    /// Returns the current SMTP configuration.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful result with the configuration, or a failure if none has been created yet.
    /// </returns>
    Task<Result<SmtpConfigResponse>> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates the SMTP configuration. Fails if one already exists.
    /// </summary>
    /// <param name="request">The configuration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful result with the created configuration, or a failure if a record already exists.
    /// </returns>
    Task<Result<SmtpConfigResponse>> CreateAsync(CreateSmtpConfigRequest request, CancellationToken ct = default);

    /// <summary>
    /// Replaces all fields of the existing SMTP configuration.
    /// </summary>
    /// <param name="request">The new configuration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful result with the updated configuration, or a failure if none has been created yet.
    /// </returns>
    Task<Result<SmtpConfigResponse>> UpdateAsync(UpdateSmtpConfigRequest request, CancellationToken ct = default);
}
