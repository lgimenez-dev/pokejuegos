using PokeJuegos.Common.Enums;

namespace PokeJuegos.Domain.Entities;

/// <summary>
/// Represents the authentication identity of a user.
/// </summary>
public sealed class User : AuditableEntity
{
    /// <summary>Gets the user's email address.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Gets the user's optional username, usable as an alternative login identifier. <see langword="null"/> if not set.</summary>
    public string? Username { get; private set; }

    /// <summary>Gets the BCrypt hash of the user's password. <see langword="null"/> for OAuth2 users.</summary>
    public string? PasswordHash { get; private set; }

    /// <summary>Gets the authorization role assigned to this user.</summary>
    public UserRole Role { get; private set; }

    /// <summary>Gets the lifecycle status of this user account.</summary>
    public UserStatus Status { get; private set; }

    /// <summary>Gets the current activation or recovery token. <see langword="null"/> when no token is active.</summary>
    public string? RefreshToken { get; private set; }

    /// <summary>Gets the expiration date of the active token. <see langword="null"/> when no token is active.</summary>
    public DateTime? ExpirationDate { get; private set; }

    /// <summary>Gets the authentication provider for this account.</summary>
    public UserProvider Provider { get; private set; }

    /// <summary>Gets the external provider user ID. <see langword="null"/> for local accounts.</summary>
    public string? ProviderId { get; private set; }

    private User() { }

    /// <summary>
    /// Creates a new <see cref="User"/> with <see cref="UserStatus.Pending"/> status.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="passwordHash">The BCrypt password hash, or <see langword="null"/> for OAuth2 users.</param>
    /// <param name="role">The authorization role to assign.</param>
    /// <param name="provider">The authentication provider.</param>
    /// <param name="providerId">The external provider user ID, or <see langword="null"/> for local accounts.</param>
    /// <param name="username">An optional username usable as an alternative login identifier.</param>
    /// <returns>A new <see cref="User"/> instance.</returns>
    public static User Create(
        string email,
        string? passwordHash,
        UserRole role,
        UserProvider provider,
        string? providerId = null,
        string? username = null)
    {
        User user = new User();
        user.InitAudit();
        user.Email = email;
        user.PasswordHash = passwordHash;
        user.Role = role;
        user.Status = UserStatus.Pending;
        user.Provider = provider;
        user.ProviderId = providerId;
        user.Username = username;
        return user;
    }

    /// <summary>
    /// Creates a new <see cref="User"/> for an OAuth2 provider with <see cref="UserStatus.Active"/> status.
    /// OAuth2 users skip the email-activation flow because the provider already verified their identity.
    /// </summary>
    /// <param name="email">The user's email address as returned by the provider.</param>
    /// <param name="role">The authorization role to assign.</param>
    /// <param name="provider">The OAuth2 provider.</param>
    /// <param name="providerId">The unique user ID issued by the provider.</param>
    /// <returns>A new <see cref="User"/> instance.</returns>
    public static User CreateOAuth2(string email, UserRole role, UserProvider provider, string providerId)
    {
        User user = new User();
        user.InitAudit();
        user.Email = email;
        user.Role = role;
        user.Status = UserStatus.Active;
        user.Provider = provider;
        user.ProviderId = providerId;
        return user;
    }

    /// <summary>
    /// Sets the activation or recovery token together with its expiration date.
    /// </summary>
    /// <param name="refreshToken">The token value to store.</param>
    /// <param name="expirationDate">The UTC date and time when the token expires.</param>
    public void UpdateToken(string refreshToken, DateTime expirationDate)
    {
        RefreshToken = refreshToken;
        ExpirationDate = expirationDate;
        SetUpdatedAt();
    }

    /// <summary>
    /// Replaces the password hash and activates the account.
    /// Called after a successful activation or password-recovery flow.
    /// </summary>
    /// <param name="passwordHash">The new BCrypt password hash.</param>
    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        Status = UserStatus.Active;
        SetUpdatedAt();
    }

    /// <summary>
    /// Activates a <see cref="UserStatus.Pending"/> account, keeping the password set at registration.
    /// </summary>
    public void Activate()
    {
        Status = UserStatus.Active;
        SetUpdatedAt();
    }

    /// <summary>
    /// Clears the active token and its expiration date.
    /// </summary>
    public void RevokeToken()
    {
        RefreshToken = null;
        ExpirationDate = null;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the username. Used by OAuth2 users, who are created without one, to complete their profile.
    /// </summary>
    /// <param name="username">The new username.</param>
    public void SetUsername(string username)
    {
        Username = username;
        SetUpdatedAt();
    }
}
