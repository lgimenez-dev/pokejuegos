namespace PokeJuegos.Mobile.Services;

/// <summary>
/// <see cref="ISessionService"/> over <see cref="SecureStorage"/> (Android Keystore, DPAPI on Windows).
/// </summary>
public class SessionService : ISessionService
{
	private const string TokenKey = "access_token";
	private const string EmailKey = "email";

	/// <inheritdoc />
	public async Task SetAsync(string token, string email)
	{
		await SecureStorage.Default.SetAsync(TokenKey, token);
		await SecureStorage.Default.SetAsync(EmailKey, email);
	}

	/// <inheritdoc />
	public async Task<string?> GetTokenAsync()
	{
		try
		{
			return await SecureStorage.Default.GetAsync(TokenKey);
		}
		catch (Exception)
		{
			// Android can lose the Keystore key (e.g. restored backup): treat it as signed out.
			Clear();
			return null;
		}
	}

	/// <inheritdoc />
	public void Clear()
	{
		SecureStorage.Default.Remove(TokenKey);
		SecureStorage.Default.Remove(EmailKey);
	}
}
