using PokeJuegos.Application.Services;

namespace PokeJuegos.Infrastructure.Services;

/// <summary>
/// Hashes and verifies passwords using BCrypt.
/// </summary>
internal sealed class PasswordService : IPasswordService
{
    /// <inheritdoc/>
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    /// <inheritdoc/>
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
