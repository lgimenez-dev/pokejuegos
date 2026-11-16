using System.Security.Cryptography;
using System.Text;

namespace PokeJuegos.Common.Helpers;

/// <summary>
/// Provides utility methods for string normalization and secure token comparison.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Normalizes a string by trimming surrounding white-space and converting to lower-case.
    /// </summary>
    /// <param name="value">The string to normalize.</param>
    /// <returns>A trimmed, lower-case representation of <paramref name="value"/>.</returns>
    public static string Normalize(string value) =>
        value.Trim().ToLowerInvariant();

    /// <summary>
    /// Compares two tokens for equality using a constant-time algorithm to prevent timing-based side-channel attacks.
    /// </summary>
    /// <param name="a">The first token to compare.</param>
    /// <param name="b">The second token to compare.</param>
    /// <returns>
    /// <see langword="true"/> when both tokens are identical; otherwise <see langword="false"/>.
    /// </returns>
    public static bool AreTokensSimilar(string a, string b)
    {
        byte[] bytesA = Encoding.UTF8.GetBytes(a);
        byte[] bytesB = Encoding.UTF8.GetBytes(b);

        return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}
