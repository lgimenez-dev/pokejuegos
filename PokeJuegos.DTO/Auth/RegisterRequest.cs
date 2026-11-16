namespace PokeJuegos.DTO;

/// <summary>
/// Request body for <c>POST /api/auth/register</c>.
/// </summary>
/// <param name="Email">The email address for the new account.</param>
/// <param name="Password">The desired plain-text password. Must be at least 8 characters, include one uppercase letter and one digit.</param>
/// <param name="Name">The user's full name.</param>
/// <param name="Username">An optional username usable as an alternative login identifier. 3-32 characters; letters, digits, dots, underscores, and hyphens only.</param>
/// <param name="NationalId">The national identity document number. Optional.</param>
/// <param name="Phone">The phone number. Optional.</param>
/// <param name="BirthDate">The date of birth. Optional.</param>
public record RegisterRequest(string? Email, string? Password, string? Name, string? Username, string? NationalId, string? Phone, DateOnly? BirthDate);
