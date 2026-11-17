using PokeJuegos.Domain;
using PokeJuegos.Domain.Entities;
using PokeJuegos.DTO;

namespace PokeJuegos.Application.UserConfigs;

/// <summary>
/// Implements <see cref="IUserConfigService"/> using <see cref="IUnitOfWork"/>.
/// </summary>
public sealed class UserConfigService(IUnitOfWork unitOfWork) : IUserConfigService
{
    private static readonly UserConfigResponse Defaults = new(Language: null, Sound: true, UpdatedAt: null);

    /// <inheritdoc/>
    public async Task<Result<UserConfigResponse>> GetAsync(Guid userId, CancellationToken ct = default)
    {
        UserConfig? config = await unitOfWork.UserConfigs.GetByUserIdAsync(userId, ct);
        return config is null ? Defaults : MapToResponse(config);
    }

    /// <inheritdoc/>
    public async Task<Result<UserConfigResponse>> SyncAsync(Guid userId, UserConfigRequest request, CancellationToken ct = default)
    {
        // timestamptz only accepts UTC; a client could send a local time with an offset.
        DateTime updatedAt = request.UpdatedAt.ToUniversalTime();

        UserConfig? config = await unitOfWork.UserConfigs.GetByUserIdAsync(userId, ct);
        if (config is null)
        {
            config = UserConfig.Create(userId, request.Language, request.Sound, updatedAt);
            await unitOfWork.UserConfigs.AddAsync(config, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
        else if (updatedAt > config.UpdatedAt)
        {
            config.Apply(request.Language, request.Sound, updatedAt);
            await unitOfWork.SaveChangesAsync(ct);
        }

        return MapToResponse(config);
    }

    /// <summary>
    /// Maps a <see cref="UserConfig"/> entity to a <see cref="UserConfigResponse"/> DTO.
    /// </summary>
    /// <param name="config">The entity to map.</param>
    /// <returns>The response with the stored preferences.</returns>
    private static UserConfigResponse MapToResponse(UserConfig config) =>
        new UserConfigResponse(config.Language, config.Sound, DateTime.SpecifyKind(config.UpdatedAt, DateTimeKind.Utc));
}
