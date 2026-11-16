using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PokeJuegos.Infrastructure.Configurations;

/// <summary>
/// Extension methods for <see cref="PropertyBuilder"/> that apply common EF Core conventions
/// used across entity configurations in this project.
/// </summary>
public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Configures an enum property to be stored as an UPPERCASE string in the database.
    /// Reads back case-insensitively. Example: <c>UserRole.Member</c> → <c>"MEMBER"</c>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="builder">The property builder to configure.</param>
    /// <param name="maxLength">Maximum column length. Defaults to 30.</param>
    /// <returns>The same builder for chaining.</returns>
    public static PropertyBuilder<TEnum> HasUpperCaseConversion<TEnum>(
        this PropertyBuilder<TEnum> builder,
        int maxLength = 30)
        where TEnum : struct, Enum
    {
        return builder
            .HasConversion(v => v.ToString().ToUpperInvariant(), s => Enum.Parse<TEnum>(s, true))
            .HasMaxLength(maxLength);
    }

    /// <summary>
    /// Configures a nullable enum property to be stored as an UPPERCASE string or <c>NULL</c> in the database.
    /// Example: <c>UserRole.Member</c> → <c>"MEMBER"</c>, <c>null</c> → <c>NULL</c>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="builder">The property builder to configure.</param>
    /// <param name="maxLength">Maximum column length. Defaults to 30.</param>
    /// <returns>The same builder for chaining.</returns>
    public static PropertyBuilder<TEnum?> HasUpperCaseConversion<TEnum>(
        this PropertyBuilder<TEnum?> builder,
        int maxLength = 30)
        where TEnum : struct, Enum
    {
        return builder
            .HasConversion(
                v => v.HasValue ? v.Value.ToString().ToUpperInvariant() : null,
                s => s != null ? Enum.Parse<TEnum>(s, true) : (TEnum?)null)
            .HasMaxLength(maxLength);
    }
}
