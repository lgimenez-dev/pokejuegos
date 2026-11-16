namespace PokeJuegos.Mobile.Services;

/// <summary>
/// The signed-in user's session (JWT), kept in the platform's secure storage.
/// </summary>
public interface ISessionService
{
	/// <summary>
	/// Stores a new session, replacing any previous one.
	/// </summary>
	/// <param name="token">The JWT returned by the API.</param>
	/// <param name="email">The signed-in user's email.</param>
	/// <returns>A task that completes when the session is stored.</returns>
	Task SetAsync(string token, string email);

	/// <summary>
	/// Reads the stored JWT.
	/// </summary>
	/// <returns>The token, or <c>null</c> if there is no session.</returns>
	Task<string?> GetTokenAsync();

	/// <summary>
	/// Removes the stored session.
	/// </summary>
	void Clear();
}
