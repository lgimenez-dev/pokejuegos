using PokeJuegos.Domain;
using PokeJuegos.Domain.Entities;
using PokeJuegos.DTO;

namespace PokeJuegos.Application.SmtpConfigs;

/// <summary>
/// Implements <see cref="ISmtpConfigService"/> using <see cref="IUnitOfWork"/>.
/// The application maintains a single SMTP configuration record at all times.
/// </summary>
public sealed class SmtpConfigService(IUnitOfWork unitOfWork) : ISmtpConfigService
{
    /// <inheritdoc/>
    public async Task<Result<SmtpConfigResponse>> GetAsync(CancellationToken ct = default)
    {
        SmtpConfig? config = await unitOfWork.SmtpConfigs.GetAsync(ct);
        if (config is null)
        {
            return ErrorMessages.SmtpConfigNotFound;
        }

        return MapToResponse(config);
    }

    /// <inheritdoc/>
    public async Task<Result<SmtpConfigResponse>> CreateAsync(
        CreateSmtpConfigRequest request, CancellationToken ct = default)
    {
        SmtpConfig? existing = await unitOfWork.SmtpConfigs.GetAsync(ct);
        if (existing is not null)
        {
            return ErrorMessages.SmtpConfigAlreadyExists;
        }

        SmtpConfig config = SmtpConfig.Create(
            request.Host!,
            request.Port,
            request.Email!,
            request.Password!,
            request.SenderName!,
            request.UseTls);

        await unitOfWork.SmtpConfigs.AddAsync(config, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(config);
    }

    /// <inheritdoc/>
    public async Task<Result<SmtpConfigResponse>> UpdateAsync(
        UpdateSmtpConfigRequest request, CancellationToken ct = default)
    {
        SmtpConfig? config = await unitOfWork.SmtpConfigs.GetAsync(ct);
        if (config is null)
        {
            return ErrorMessages.SmtpConfigNotFound;
        }

        config.Update(
            request.Host!,
            request.Port,
            request.Email!,
            request.Password!,
            request.SenderName!,
            request.UseTls);

        await unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(config);
    }

    /// <summary>
    /// Maps a <see cref="SmtpConfig"/> entity to a <see cref="SmtpConfigResponse"/> DTO.
    /// Password is intentionally excluded from the response.
    /// </summary>
    /// <param name="config">The entity to map.</param>
    /// <returns>A <see cref="SmtpConfigResponse"/> with the config's public fields.</returns>
    private static SmtpConfigResponse MapToResponse(SmtpConfig config) =>
        new SmtpConfigResponse(
            config.Id,
            config.Host,
            config.Port,
            config.Email,
            config.SenderName,
            config.UseTls);
}
