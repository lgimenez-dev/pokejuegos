namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Runs an OAuth2 sign-in in the system browser and captures the redirect back to the app.
/// Each platform gets the token back differently: Android through the <c>pokejuegos://oauth</c> custom scheme
/// (<c>WebAuthenticator</c>), Windows through a loopback listener, since <c>WebAuthenticator</c> doesn't work on WinUI.
/// </summary>
public interface IOAuthBrowser
{
	/// <summary>
	/// Opens the browser and waits for the redirect.
	/// </summary>
	/// <param name="buildStartUrl">Builds the URL to open from this platform's return URL (the backend redirects there when done).</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>The query parameters of the redirect, or <c>null</c> if the user cancelled or it timed out.</returns>
	Task<IReadOnlyDictionary<string, string>?> AuthenticateAsync(Func<string, Uri> buildStartUrl, CancellationToken ct);
}
