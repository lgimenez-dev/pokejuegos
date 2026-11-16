namespace PokeJuegos.DTO;

/// <summary>
/// Response body for <c>GET /api/auth/me</c>, containing the identity of the currently authenticated user.
/// Values are read directly from the JWT claims.
/// </summary>
/// <param name="UserId">The user's unique identifier (JWT <c>sub</c> claim).</param>
/// <param name="Name">The user's full name (JWT <c>name</c> claim).</param>
/// <param name="Email">The user's email address (JWT <c>email</c> claim).</param>
/// <param name="Role">The user's authorization role (JWT <c>role</c> claim).</param>
public record MeResponse(string UserId, string Name, string Email, string Role);
