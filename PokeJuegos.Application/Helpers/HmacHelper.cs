using System.Security.Cryptography;
using System.Text;

namespace PokeJuegos.Application.Helpers;

/// <summary>
/// Provides HMAC-SHA256 hashing and constant-time validation for token security.
/// </summary>
public static class HmacHelper
{
    /// <summary>
    /// Computes an HMAC-SHA256 hash of <paramref name="data"/> using a hex-encoded secret key.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="secretHex">The secret key encoded as a hexadecimal string.</param>
    /// <returns>The HMAC-SHA256 hash encoded as an uppercase hexadecimal string.</returns>
    public static string HashToken(string data, string secretHex)
    {
        byte[] keyBytes = Convert.FromHexString(secretHex);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] hashBytes = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Verifies that the HMAC-SHA256 hash of <paramref name="data"/> matches <paramref name="storedHash"/>
    /// using a constant-time comparison to prevent timing attacks.
    /// </summary>
    /// <param name="data">The data to hash and compare.</param>
    /// <param name="secretHex">The secret key encoded as a hexadecimal string.</param>
    /// <param name="storedHash">The previously stored hash to compare against.</param>
    /// <returns><see langword="true"/> if the hashes match; otherwise <see langword="false"/>.</returns>
    public static bool ValidateToken(string data, string secretHex, string storedHash)
    {
        string computedHash = HashToken(data, secretHex);
        byte[] computedBytes = Encoding.UTF8.GetBytes(computedHash);
        byte[] storedBytes = Encoding.UTF8.GetBytes(storedHash);

        if (computedBytes.Length != storedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(computedBytes, storedBytes);
    }
}
