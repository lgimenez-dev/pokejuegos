using PokeJuegos.Domain;
using PokeJuegos.DTO;

namespace PokeJuegos.Application.UserConfigs;

/// <summary>
/// Reads and syncs the preferences of the current user (language, sound).
/// </summary>
public interface IUserConfigService
{
    /// <summary>
    /// Returns the user's preferences, or the defaults (no language, sound on, no date) if they never saved any.
    /// </summary>
    /// <param name="userId">The current user's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see cref="Result{T}.Success"/> with the stored preferences.</returns>
    Task<Result<UserConfigResponse>> GetAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Saves the client's copy if there is none on the server or it is newer than the stored one (last write wins).
    /// </summary>
    /// <param name="userId">The current user's ID.</param>
    /// <param name="request">The whole configuration as the client has it.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="Result{T}.Success"/> with what the server holds after the call: the client's copy if it won,
    /// or the newer server copy, which the client should then apply.
    /// </returns>
    Task<Result<UserConfigResponse>> SyncAsync(Guid userId, UserConfigRequest request, CancellationToken ct = default);
}
