namespace PokeJuegos.DTO;

/// <summary>
/// Response body returned when a session token is confirmed as valid.
/// </summary>
/// <param name="Token">The validated JWT that was checked against the active session store.</param>
public record ValidateTokenResponse(string Token);
