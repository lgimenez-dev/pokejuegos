namespace PokeJuegos.DTO;

/// <summary>
/// Request body for <c>POST /api/auth/recover-password</c>.
/// The response is always <c>200 OK</c> regardless of whether the email exists, to prevent user enumeration.
/// </summary>
/// <param name="Email">The email address associated with the account.</param>
public record RecoverPasswordRequest(string? Email);
