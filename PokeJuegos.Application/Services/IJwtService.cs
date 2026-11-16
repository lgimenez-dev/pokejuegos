using PokeJuegos.Common.Enums;

namespace PokeJuegos.Application.Services;

/// <summary>
/// Defines JWT generation for authenticated users.
/// </summary>
public interface IJwtService
{
    /// <summary>Gets the token lifetime in minutes, as configured in <c>Jwt:ExpirationMinutes</c>.</summary>
    int ExpirationMinutes { get; }

    /// <summary>
    /// Generates a signed JWT containing the user's identity claims.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="name">The user's full name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="role">The user's authorization role.</param>
    /// <returns>A signed JWT string.</returns>
    string GenerateToken(Guid userId, string name, string email, UserRole role);
}
