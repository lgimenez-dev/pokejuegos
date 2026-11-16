namespace PokeJuegos.DTO;

/// <summary>
/// Request body for <c>POST /api/auth/login</c>.
/// </summary>
/// <param name="Email">The user's email address. Provide either <c>Email</c> or <c>Username</c>, not both.</param>
/// <param name="Username">The user's username. Provide either <c>Email</c> or <c>Username</c>, not both.</param>
/// <param name="Password">The user's plain-text password.</param>
public record LoginRequest(string? Email, string? Username, string? Password);
