using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile;

/// <summary>
/// <see cref="IOAuthBrowser"/> on Android: <see cref="WebAuthenticator"/> opens a Custom Tab and
/// <see cref="WebAuthenticationCallbackActivity"/> receives the <c>pokejuegos://oauth</c> redirect.
/// </summary>
public class WebAuthenticatorOAuthBrowser : IOAuthBrowser
{
	public const string CallbackUrl = "pokejuegos://oauth";

	/// <inheritdoc />
	public async Task<IReadOnlyDictionary<string, string>?> AuthenticateAsync(Func<string, Uri> buildStartUrl, CancellationToken ct)
	{
		try
		{
			WebAuthenticatorResult result = await WebAuthenticator.Default.AuthenticateAsync(
				new WebAuthenticatorOptions
				{
					Url = buildStartUrl(CallbackUrl),
					CallbackUrl = new Uri(CallbackUrl),
				},
				ct);
			return result.Properties;
		}
		catch (TaskCanceledException)
		{
			// The user closed the Custom Tab, or the sign-in was cancelled from the app.
			return null;
		}
	}
}
