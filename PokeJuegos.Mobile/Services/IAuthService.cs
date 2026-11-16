using PokeJuegos.DTO;
using PokeJuegos.Mobile.Models;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Authentication operations, shaped after <c>/api/auth</c> of <c>PokeJuegos.Api</c>.
/// </summary>
public interface IAuthService
{
	/// <summary>
	/// Authenticates a user and stores the session on success.
	/// </summary>
	/// <param name="request">The credentials: either <c>Email</c> or <c>Username</c>, plus <c>Password</c>.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>The session data, or the reason it was rejected (invalid credentials, pending account…).</returns>
	Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);

	/// <summary>
	/// Creates an account. The API leaves it pending until it is activated from the emailed link.
	/// </summary>
	/// <param name="request">The registration details.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns><c>true</c> on success, or the validation/business errors.</returns>
	Task<AuthResult<bool>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

	/// <summary>
	/// Signs in with Google in the system browser and stores the session on success.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>
	/// The session data (check <see cref="LoginResponse.ProfileComplete"/>), the error, or <c>null</c> if the user cancelled.
	/// </returns>
	Task<AuthResult<LoginResponse>?> LoginWithGoogleAsync(CancellationToken ct = default);

	/// <summary>
	/// Reads the signed-in user's identity (<c>GET /api/auth/me</c>).
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>The user's id, name, email and role, or the error.</returns>
	Task<AuthResult<MeResponse>> GetMeAsync(CancellationToken ct = default);

	/// <summary>
	/// Sets the username and name of a user who signed up with Google (<c>PUT /api/auth/profile</c>).
	/// </summary>
	/// <param name="request">The username and name.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns><c>true</c> on success, or the validation/business errors.</returns>
	Task<AuthResult<bool>> CompleteProfileAsync(CompleteProfileRequest request, CancellationToken ct = default);

	/// <summary>
	/// Asks the API to email a link to reset the password, or to create one for an account that signed up with Google
	/// (<c>POST /api/auth/recover-password</c>). The API answers the same whether or not the account exists.
	/// </summary>
	/// <param name="request">The account's email.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns><c>true</c> on success, or the validation/rate-limit errors.</returns>
	Task<AuthResult<bool>> RecoverPasswordAsync(RecoverPasswordRequest request, CancellationToken ct = default);
}
