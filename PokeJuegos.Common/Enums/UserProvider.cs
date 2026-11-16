namespace PokeJuegos.Common.Enums;

/// <summary>
/// Represents the identity provider used to authenticate a user.
/// </summary>
public enum UserProvider
{
    /// <summary>
    /// Authentication is handled locally using email and password.
    /// </summary>
    Local,

    /// <summary>
    /// Authentication is delegated to Google OAuth2.
    /// </summary>
    Google,

    /// <summary>
    /// Authentication is delegated to GitHub OAuth2.
    /// </summary>
    GitHub
}
