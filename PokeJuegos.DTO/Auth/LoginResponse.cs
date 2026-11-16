namespace PokeJuegos.DTO;

/// <summary>
/// Response body returned on a successful login.
/// </summary>
/// <param name="Token">The signed JWT for the new session.</param>
/// <param name="Email">The authenticated user's email address.</param>
/// <param name="Role">The authenticated user's authorization role.</param>
/// <param name="ProfileComplete">Whether the user has a username. OAuth2 users start without one and must complete their profile via <c>PUT /api/auth/profile</c>.</param>
public record LoginResponse(string Token, string Email, string Role, bool ProfileComplete);
