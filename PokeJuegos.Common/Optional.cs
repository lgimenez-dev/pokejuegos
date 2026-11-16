namespace PokeJuegos.Common;

/// <summary>
/// Represents an optional value that may or may not be present.
/// Used to distinguish between "value not provided" and "value provided as null".
/// </summary>
/// <typeparam name="T">The type of the contained value.</typeparam>
public readonly struct Optional<T>
{
    private readonly T? _value;

    /// <summary>
    /// Gets a value indicating whether this instance contains a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the contained value when <see cref="HasValue"/> is <see langword="true"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed and <see cref="HasValue"/> is <see langword="false"/>.
    /// </exception>
    public T Value
    {
        get
        {
            if (!HasValue)
            {
                throw new InvalidOperationException("Optional<T> has no value.");
            }

            return _value!;
        }
    }

    private Optional(T value)
    {
        _value = value;
        HasValue = true;
    }

    /// <summary>
    /// Creates an <see cref="Optional{T}"/> instance that contains the specified value.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>An <see cref="Optional{T}"/> with <see cref="HasValue"/> set to <see langword="true"/>.</returns>
    public static Optional<T> Of(T value) => new(value);

    /// <summary>
    /// Returns an <see cref="Optional{T}"/> instance that contains no value.
    /// </summary>
    /// <returns>An <see cref="Optional{T}"/> with <see cref="HasValue"/> set to <see langword="false"/>.</returns>
    public static Optional<T> Empty() => default;

    /// <summary>
    /// Returns the contained value when present, or <paramref name="fallback"/> when absent.
    /// </summary>
    /// <param name="fallback">The value to return when this instance has no value.</param>
    /// <returns>The contained value or <paramref name="fallback"/>.</returns>
    public T GetValueOrDefault(T fallback) => HasValue ? _value! : fallback;

    /// <summary>
    /// Returns a string representation of the optional value.
    /// </summary>
    /// <returns>
    /// The string representation of the contained value when present;
    /// otherwise the literal string <c>"Optional.Empty"</c>.
    /// </returns>
    public override string ToString() =>
        HasValue ? _value?.ToString() ?? string.Empty : "Optional.Empty";
}
