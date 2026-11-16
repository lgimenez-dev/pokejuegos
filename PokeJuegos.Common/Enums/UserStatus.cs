namespace PokeJuegos.Common.Enums;

/// <summary>
/// Represents the lifecycle status of a user account.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// The account is fully active and the user may authenticate.
    /// </summary>
    Active,

    /// <summary>
    /// The account has been created but not yet confirmed (e.g. awaiting email verification).
    /// </summary>
    Pending,

    /// <summary>
    /// The account has been deactivated and the user may not authenticate.
    /// </summary>
    Inactive
}
