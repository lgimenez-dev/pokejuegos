namespace PokeJuegos.Web.Services;

/// <summary>
/// Outcome of a call to <c>PokeJuegos.Api</c>: success, or the error codes to translate and show (general and/or per field).
/// </summary>
/// <param name="IsSuccess">Whether the API accepted the request.</param>
/// <param name="Error">A general error message, or <see langword="null"/>.</param>
/// <param name="FieldErrors">Validation messages keyed by request property (e.g. <c>Password</c>).</param>
public record ApiResult(bool IsSuccess, string? Error, IReadOnlyDictionary<string, string> FieldErrors)
{
    /// <summary>A successful result.</summary>
    public static ApiResult Success { get; } = new(true, null, new Dictionary<string, string>());

    /// <summary>Creates a failure with a general message.</summary>
    /// <param name="error">The message to show.</param>
    /// <returns>The failed result.</returns>
    public static ApiResult Failure(string error) => new(false, error, new Dictionary<string, string>());

    /// <summary>Creates a validation failure.</summary>
    /// <param name="fieldErrors">Messages keyed by request property.</param>
    /// <param name="error">An object-level message, if any.</param>
    /// <returns>The failed result.</returns>
    public static ApiResult Invalid(IReadOnlyDictionary<string, string> fieldErrors, string? error) => new(false, error, fieldErrors);
}
