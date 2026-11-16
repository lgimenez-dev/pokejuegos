using PokeJuegos.Domain;
using PokeJuegos.DTO;

namespace PokeJuegos.Application.Auth;

/// <summary>
/// Defines the authentication and account-lifecycle operations exposed by the auth feature.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Creates a new user account with <c>Pending</c> status and sends an activation email if SMTP is configured.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="Result{T}.Success"/> with <see langword="true"/> on success, or
    /// <see cref="Result{T}.Failure"/> if the email address is already registered.
    /// </returns>
    Task<Result<bool>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>
    /// Authenticates a user, revokes any pre-existing sessions, and issues a new JWT session.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="Result{T}.Success"/> with the token and user identity on success, or
    /// <see cref="Result{T}.Failure"/> for invalid credentials, pending account, or inactive account.
    /// </returns>
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// Revokes the session associated with the given JWT, effectively logging the user out.
    /// </summary>
    /// <param name="token">The raw JWT string to revoke.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="Result{T}.Success"/> with <see langword="true"/> on success, or
    /// <see cref="Result{T}.Failure"/> if the session is not found or already revoked.
    /// </returns>
    Task<Result<bool>> LogoutAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Confirms that the given JWT corresponds to a valid, non-revoked, non-expired session.
    /// </summary>
    /// <param name="token">The raw JWT string to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="Result{T}.Success"/> with the token on success, or
    /// <see cref="Result{T}.Failure"/> if the session is not found, revoked, or expired.
    /// </returns>
    Task<Result<ValidateTokenResponse>> ValidateTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Initiates a password-recovery flow by sending a time-limited token to the user's email.
    /// OAuth2 accounts without a password get the same link to create one.
    /// Always returns success to prevent user enumeration, even when the account is not found.
    /// </summary>
    /// <param name="request">The email used to identify the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see cref="Result{T}.Success"/> with <see langword="true"/> in all cases.</returns>
    Task<Result<bool>> RecoverPasswordAsync(RecoverPasswordRequest request, CancellationToken ct = default);

    /// <summary>
    /// Activates a <c>Pending</c> account using the one-time token sent in the registration email.
    /// </summary>
    /// <param name="request">The activation token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="Result{T}.Success"/> with <see langword="true"/> on success, or
    /// <see cref="Result{T}.Failure"/> if the token is invalid, expired, or not an activation token.
    /// </returns>
    Task<Result<bool>> ActivateAsync(ActivateAccountRequest request, CancellationToken ct = default);

    /// <summary>
    /// Sets a new password using a one-time token received by email.
    /// Activates the account if it was in <c>Pending</c> status.
    /// </summary>
    /// <param name="request">The one-time token and the new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="Result{T}.Success"/> with <see langword="true"/> on success, or
    /// <see cref="Result{T}.Failure"/> if the token is invalid or expired.
    /// </returns>
    Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default);

    /// <summary>
    /// Sets the username and name of a user created without a username (OAuth2 sign-up).
    /// </summary>
    /// <param name="userId">The ID of the authenticated user.</param>
    /// <param name="request">The username and name to set.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="Result{T}.Success"/> with <see langword="true"/> on success, or
    /// <see cref="Result{T}.Failure"/> if the user doesn't exist, already has a username, or the username is taken.
    /// </returns>
    Task<Result<bool>> CompleteProfileAsync(Guid userId, CompleteProfileRequest request, CancellationToken ct = default);
}
