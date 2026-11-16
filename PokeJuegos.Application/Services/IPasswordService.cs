namespace PokeJuegos.Application.Services;

/// <summary>
/// Defines BCrypt password hashing and verification.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Returns a BCrypt hash of the given plain-text password.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A BCrypt hash string.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain-text password against a BCrypt hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hash">The stored BCrypt hash to verify against.</param>
    /// <returns><see langword="true"/> if the password matches the hash; otherwise <see langword="false"/>.</returns>
    bool Verify(string password, string hash);
}
