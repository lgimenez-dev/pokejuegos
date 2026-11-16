namespace PokeJuegos.Common.Enums;

/// <summary>
/// Represents the authorization role assigned to a user account.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standard authenticated user with no elevated permissions.
    /// </summary>
    Member,

    /// <summary>
    /// Administrator with full access to all resources.
    /// </summary>
    Admin
}
