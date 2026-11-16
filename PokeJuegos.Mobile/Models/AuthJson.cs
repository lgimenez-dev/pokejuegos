using System.Text.Json;
using System.Text.Json.Serialization;
using PokeJuegos.DTO;

namespace PokeJuegos.Mobile.Models;

/// <summary>Error body of a business-rule failure (copy of <c>PokeJuegos.Api.Models.BusinessErrorResponse</c>, internal to the API).</summary>
/// <param name="Message">The user-facing error message.</param>
public record BusinessErrorResponse(string Message);

/// <summary>One failed rule of a validation error (copy of <c>PokeJuegos.Api.Models.FieldError</c>).</summary>
/// <param name="Field">The request property that failed (e.g. <c>Email</c>), or <c>null</c> for object-level rules.</param>
/// <param name="Message">The validation message.</param>
public record FieldError(string? Field, string Message);

/// <summary>Error body of a validation failure (copy of <c>PokeJuegos.Api.Models.ValidationErrorResponse</c>).</summary>
/// <param name="Errors">The failed rules.</param>
public record ValidationErrorResponse(IReadOnlyList<FieldError> Errors);

// Source-generated serialization: reflection-based JsonSerializer calls break in the trimmed Release APK.
// camelCase both ways, like ASP.NET Core's defaults.
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(RegisterRequest))]
[JsonSerializable(typeof(CompleteProfileRequest))]
[JsonSerializable(typeof(RecoverPasswordRequest))]
[JsonSerializable(typeof(MeResponse))]
[JsonSerializable(typeof(BusinessErrorResponse))]
[JsonSerializable(typeof(ValidationErrorResponse))]
internal sealed partial class AuthJsonContext : JsonSerializerContext;
