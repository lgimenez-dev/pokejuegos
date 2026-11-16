namespace PokeJuegos.Api.Models;

/// <summary>
/// Represents the response body returned when a business rule violation occurs.
/// Shape: <c>{ "message": "..." }</c>
/// </summary>
/// <param name="Message">The human-readable error message describing the violation.</param>
record BusinessErrorResponse(string Message);
