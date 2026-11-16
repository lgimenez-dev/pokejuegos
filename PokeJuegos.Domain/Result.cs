namespace PokeJuegos.Domain;

/// <summary>
/// Represents the outcome of an operation that can either succeed or fail.
/// Services return <see cref="Result{T}"/> instead of throwing exceptions for business logic errors.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T>
{
    /// <summary>Gets the result value on success; <see langword="null"/> on failure.</summary>
    public T? Value { get; }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets the single error message on failure; <see langword="null"/> on success.</summary>
    public string? Error { get; }

    /// <summary>Gets the list of error messages. Empty on success or when a single error is used.</summary>
    public List<string> ListErrors { get; }

    private Result(T? value, bool isSuccess, string? error, List<string>? errors = null)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
        ListErrors = errors ?? [];
    }

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The operation result value.</param>
    /// <returns>A <see cref="Result{T}"/> representing success.</returns>
    public static Result<T> Success(T value) => new(value, true, null);

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <returns>A <see cref="Result{T}"/> representing failure.</returns>
    public static Result<T> Failure(string error) => new(default, false, error);

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    /// <param name="errors">The list of error messages describing the failure.</param>
    /// <returns>A <see cref="Result{T}"/> representing failure.</returns>
    public static Result<T> Failure(List<string> errors) => new(default, false, null, errors);

    /// <summary>
    /// Implicitly converts a value to a successful <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The operation result value.</param>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts an error string to a failed <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    public static implicit operator Result<T>(string error) => Failure(error);
}
