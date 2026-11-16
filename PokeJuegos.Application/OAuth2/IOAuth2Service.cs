using PokeJuegos.Domain;
using PokeJuegos.DTO;

namespace PokeJuegos.Application.OAuth2;

/// <summary>
/// Defines the application-level OAuth2 callback handling.
/// </summary>
public interface IOAuth2Service
{
    /// <summary>
    /// Upserts a user from a Google OAuth2 callback and issues a new session.
    /// Creates the user as <c>Active</c> on first login; returns a failure if the email is already
    /// registered under a different provider.
    /// </summary>
    /// <param name="email">The email address returned by Google.</param>
    /// <param name="name">The display name returned by Google.</param>
    /// <param name="providerId">The unique Google user ID (<c>sub</c> claim).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful result with a <see cref="LoginResponse"/>, or a failure if the email belongs to
    /// a local account.
    /// </returns>
    Task<Result<LoginResponse>> HandleGoogleCallbackAsync(
        string email, string name, string providerId, CancellationToken ct = default);
}
