using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.Models;

/// <summary>
/// Outcome of an <see cref="Services.IAuthService"/> call: either a value, or the errors to show.
/// Failures are created from error codes (the API's or the app's) and translated here with <see cref="ErrorText"/>.
/// </summary>
/// <typeparam name="T">The success value.</typeparam>
public sealed class AuthResult<T>
{
	private static readonly IReadOnlyDictionary<string, string> NoFieldErrors = new Dictionary<string, string>();

	private AuthResult(T? value, string? errorCode, IReadOnlyDictionary<string, string> fieldErrors)
	{
		Value = value;
		ErrorCode = errorCode;
		Error = errorCode is null ? null : ErrorText.For(errorCode);
		FieldErrors = fieldErrors;
	}

	public T? Value { get; }

	/// <summary>The code of the general error (e.g. <c>USERNAME_ALREADY_TAKEN</c>), to react to specific ones.</summary>
	public string? ErrorCode { get; }

	/// <summary>A general message (business rule, network, rate limit), not tied to a field, already translated.</summary>
	public string? Error { get; }

	/// <summary>
	/// Translated validation messages keyed by request property name (<c>Email</c>, <c>Username</c>…), first message per field.
	/// </summary>
	public IReadOnlyDictionary<string, string> FieldErrors { get; }

	public bool IsSuccess => Error is null && FieldErrors.Count == 0;

	public static AuthResult<T> Success(T value) => new(value, null, NoFieldErrors);

	/// <param name="errorCode">The error code, e.g. <c>INVALID_CREDENTIALS</c> or <see cref="ErrorText.ServerUnreachable"/>.</param>
	public static AuthResult<T> Failure(string errorCode) => new(default, errorCode, NoFieldErrors);

	/// <param name="fieldErrorCodes">Error codes keyed by request property name.</param>
	/// <param name="errorCode">The code of an object-level error, if any.</param>
	public static AuthResult<T> Invalid(IReadOnlyDictionary<string, string> fieldErrorCodes, string? errorCode = null) =>
		new(default, errorCode, fieldErrorCodes.ToDictionary(pair => pair.Key, pair => ErrorText.For(pair.Value)));
}
