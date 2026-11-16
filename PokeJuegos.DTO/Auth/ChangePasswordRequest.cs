namespace PokeJuegos.DTO;

/// <summary>
/// Request body for <c>POST /api/auth/change-password</c>.
/// Used by the password-recovery flow (activation has its own <see cref="ActivateAccountRequest"/>).
/// </summary>
/// <param name="Token">The one-time token received by email.</param>
/// <param name="Password">The new plain-text password. Must be at least 8 characters, include one uppercase letter and one digit.</param>
public record ChangePasswordRequest(string? Token, string? Password);
