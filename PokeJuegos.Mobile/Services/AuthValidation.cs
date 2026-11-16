using System.Text.RegularExpressions;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Client-side copies of the API's account rules (<c>RegisterValidator</c>, <c>CompleteProfileValidator</c>),
/// so forms show errors before the request. Each method returns the message, or an empty string if the value is valid.
/// </summary>
public static partial class AuthValidation
{
	// Business errors of the API that belong to a field (PokeJuegos.Application/ErrorMessages.cs).
	public const string EmailAlreadyRegistered = "An account with this email address already exists.";
	public const string UsernameAlreadyRegistered = "This username is already taken.";

	public static string Name(string name) =>
		name.Trim().Length == 0 ? "Enter your name." : string.Empty;

	public static string Username(string username) =>
		UsernameRegex().IsMatch(username.Trim())
			? string.Empty
			: "3-32 characters: letters, digits, dots, underscores and hyphens.";

	// Same check as FluentValidation's EmailAddress(): one '@', not at either end.
	public static string Email(string email)
	{
		string trimmed = email.Trim();
		int at = trimmed.IndexOf('@');
		return at > 0 && at == trimmed.LastIndexOf('@') && at < trimmed.Length - 1
			? string.Empty
			: "Enter a valid email address.";
	}

	public static string Password(string password) =>
		password.Length >= 8 && password.Any(char.IsAsciiLetterUpper) && password.Any(char.IsAsciiDigit)
			? string.Empty
			: "At least 8 characters, with an uppercase letter and a digit.";

	[GeneratedRegex("^[A-Za-z0-9._-]{3,32}$")]
	private static partial Regex UsernameRegex();
}
