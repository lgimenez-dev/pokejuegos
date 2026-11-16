namespace PokeJuegos.DTO;

/// <summary>
/// Request body for <c>PUT /api/auth/profile</c>, sent by OAuth2 users after their first sign-in.
/// </summary>
/// <param name="Username">The username to set. 3-32 characters; letters, digits, dots, underscores, and hyphens only.</param>
/// <param name="Name">The user's full name.</param>
public record CompleteProfileRequest(string? Username, string? Name);
