using System.Security.Cryptography;

namespace PokeJuegos.Common.Helpers;

/// <summary>
/// Provides utility methods for generating random numeric values.
/// </summary>
public static class NumberHelper
{
    /// <summary>
    /// Generates a cryptographically random numeric string of the specified length.
    /// Suitable for use as OTP codes, activation codes, and similar tokens.
    /// </summary>
    /// <param name="length">The number of digits to generate. Must be greater than zero.</param>
    /// <returns>A string containing exactly <paramref name="length"/> random decimal digits (0–9).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="length"/> is less than or equal to zero.
    /// </exception>
    public static string GenerateRandomNumericString(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");
        }

        char[] digits = new char[length];
        byte[] randomBytes = RandomNumberGenerator.GetBytes(length);

        for (int i = 0; i < length; i++)
        {
            digits[i] = (char)('0' + randomBytes[i] % 10);
        }

        return new string(digits);
    }
}
