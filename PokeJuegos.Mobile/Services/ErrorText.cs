using PokeJuegos.Mobile.Resources.Strings;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Turns error codes (the API's <c>ErrorMessages</c>, e.g. <c>USERNAME_ALREADY_TAKEN</c>, or the app's own,
/// e.g. <c>SERVER_UNREACHABLE</c>) into the message to show, through the <c>Error_&lt;CODE&gt;</c> resources.
/// </summary>
public static class ErrorText
{
	public const string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
	public const string UsernameAlreadyTaken = "USERNAME_ALREADY_TAKEN";
	public const string ServerUnreachable = "SERVER_UNREACHABLE";
	public const string TooManyAttempts = "TOO_MANY_ATTEMPTS";
	public const string SessionExpired = "SESSION_EXPIRED";
	public const string UnexpectedError = "UNEXPECTED_ERROR";

	/// <summary>
	/// The message for an error code in the current language.
	/// </summary>
	/// <param name="code">The error code.</param>
	/// <returns>The translated message, or the generic error if the code has no text (e.g. a newer API).</returns>
	public static string For(string code) =>
		AppResources.ResourceManager.GetString("Error_" + code, AppResources.Culture)
		?? AppResources.Error_UNEXPECTED_ERROR;
}
