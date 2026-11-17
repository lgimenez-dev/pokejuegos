using System.Text.RegularExpressions;
using PokeJuegos.Mobile.Resources.Strings;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Client-side copies of the API's account rules (<c>RegisterValidator</c>, <c>CompleteProfileValidator</c>),
/// so forms show errors before the request. Each method returns the message, or an empty string if the value is valid.
/// </summary>
public static partial class AuthValidation
{
	public static string Name(string name) =>
		name.Trim().Length == 0 ? AppResources.Validation_Name : string.Empty;

	public static string Username(string username) =>
		UsernameRegex().IsMatch(username.Trim())
			? string.Empty
			: AppResources.Error_INVALID_USERNAME;

	// Same check as FluentValidation's EmailAddress(): one '@', not at either end.
	public static string Email(string email)
	{
		string trimmed = email.Trim();
		int at = trimmed.IndexOf('@');
		return at > 0 && at == trimmed.LastIndexOf('@') && at < trimmed.Length - 1
			? string.Empty
			: AppResources.Error_INVALID_EMAIL;
	}

	public static string Password(string password) =>
		password.Length >= 8 && password.Any(char.IsAsciiLetterUpper) && password.Any(char.IsAsciiDigit)
			? string.Empty
			: AppResources.Validation_Password;

	[GeneratedRegex("^[A-Za-z0-9._-]{3,32}$")]
	private static partial Regex UsernameRegex();
}
