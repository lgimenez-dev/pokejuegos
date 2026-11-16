namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Where <c>PokeJuegos.Api</c> lives.
/// </summary>
public static class ApiSettings
{
	/// <summary>
	/// The API's http profile (<c>launchSettings.json</c>). The Android emulator reaches it through
	/// <c>adb reverse tcp:5224 tcp:5224</c> rather than <c>10.0.2.2</c>, because Google OAuth only accepts
	/// <c>localhost</c> as a development redirect URI. Cleartext HTTP is allowed only for localhost
	/// (<c>Platforms/Android/Resources/xml/network_security_config.xml</c>).
	/// </summary>
	public const string BaseUrl = "http://localhost:5224/";
}
