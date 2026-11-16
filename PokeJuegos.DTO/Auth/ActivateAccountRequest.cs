namespace PokeJuegos.DTO;

/// <summary>
/// Request body for account activation via the token sent in the registration email.
/// </summary>
/// <param name="Token">The activation token received by email.</param>
public record ActivateAccountRequest(string? Token);
