namespace PokeJuegos.Mobile.Models;

/// <summary>
/// Outcome of an <see cref="Services.IAuthService"/> call: either a value, or the errors to show.
/// </summary>
/// <typeparam name="T">The success value.</typeparam>
public sealed class AuthResult<T>
{
	private static readonly IReadOnlyDictionary<string, string> NoFieldErrors = new Dictionary<string, string>();

	private AuthResult(T? value, string? error, IReadOnlyDictionary<string, string> fieldErrors)
	{
		Value = value;
		Error = error;
		FieldErrors = fieldErrors;
	}

	public T? Value { get; }

	/// <summary>A general message (business rule, network, rate limit), not tied to a field.</summary>
	public string? Error { get; }

	/// <summary>Validation messages keyed by request property name (<c>Email</c>, <c>Username</c>…), first message per field.</summary>
	public IReadOnlyDictionary<string, string> FieldErrors { get; }

	public bool IsSuccess => Error is null && FieldErrors.Count == 0;

	public static AuthResult<T> Success(T value) => new(value, null, NoFieldErrors);

	public static AuthResult<T> Failure(string error) => new(default, error, NoFieldErrors);

	public static AuthResult<T> Invalid(IReadOnlyDictionary<string, string> fieldErrors, string? error = null) =>
		new(default, error, fieldErrors);
}
