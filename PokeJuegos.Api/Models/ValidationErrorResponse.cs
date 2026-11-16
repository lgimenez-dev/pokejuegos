using FluentValidation.Results;

namespace PokeJuegos.Api.Models;

/// <summary>
/// A single field-level validation error.
/// </summary>
/// <param name="Field">The name of the field that failed validation, or <see langword="null"/> for model-level errors.</param>
/// <param name="Message">The human-readable description of the validation failure.</param>
record FieldError(string? Field, string Message);

/// <summary>
/// Represents the response body returned when request validation fails.
/// Shape: <c>{ "errors": [{ "field": "...", "message": "..." }] }</c>
/// </summary>
/// <param name="Errors">The list of field-level validation errors.</param>
record ValidationErrorResponse(IReadOnlyList<FieldError> Errors)
{
    /// <summary>
    /// Creates a <see cref="ValidationErrorResponse"/> from a FluentValidation <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="validation">The failed <see cref="ValidationResult"/> produced by a validator.</param>
    /// <returns>A <see cref="ValidationErrorResponse"/> containing all validation failures.</returns>
    public static ValidationErrorResponse From(ValidationResult validation) =>
        new(validation.Errors.Select(e => new FieldError(e.PropertyName, e.ErrorMessage)).ToList());
}
