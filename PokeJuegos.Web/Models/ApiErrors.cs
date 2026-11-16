namespace PokeJuegos.Web.Models;

/// <summary>Error body of a business-rule failure (copy of <c>PokeJuegos.Api.Models.BusinessErrorResponse</c>, internal to the API).</summary>
/// <param name="Message">The user-facing error message.</param>
public record BusinessErrorResponse(string Message);

/// <summary>One failed rule of a validation error (copy of <c>PokeJuegos.Api.Models.FieldError</c>).</summary>
/// <param name="Field">The request property that failed (e.g. <c>Password</c>), or <c>null</c> for object-level rules.</param>
/// <param name="Message">The validation message.</param>
public record FieldError(string? Field, string Message);

/// <summary>Error body of a validation failure (copy of <c>PokeJuegos.Api.Models.ValidationErrorResponse</c>).</summary>
/// <param name="Errors">The failed rules.</param>
public record ValidationErrorResponse(IReadOnlyList<FieldError> Errors);
