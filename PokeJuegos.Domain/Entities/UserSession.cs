namespace PokeJuegos.Domain.Entities;

/// <summary>
/// Represents an active JWT session for a user.
/// </summary>
public sealed class UserSession : AuditableEntity
{
    /// <summary>Gets the ID of the user who owns this session.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the HMAC-SHA256 hash of the JWT used to identify this session.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>Gets the User-Agent header captured at login. <see langword="null"/> if not available.</summary>
    public string? UserAgent { get; private set; }

    /// <summary>Gets the client IP address captured at login. <see langword="null"/> if not available.</summary>
    public string? Ip { get; private set; }

    /// <summary>Gets the UTC date and time when this session expires.</summary>
    public DateTime ExpirationDate { get; private set; }

    /// <summary>Gets the UTC date and time when this session was revoked. <see langword="null"/> if still active.</summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this session is currently valid (not revoked and not expired).
    /// </summary>
    public bool IsValid => !RevokedAt.HasValue && ExpirationDate > DateTime.UtcNow;

    private UserSession() { }

    /// <summary>
    /// Creates a new <see cref="UserSession"/> for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the owning user.</param>
    /// <param name="tokenHash">The HMAC-SHA256 hash of the issued JWT.</param>
    /// <param name="userAgent">The client User-Agent header, or <see langword="null"/>.</param>
    /// <param name="ip">The client IP address, or <see langword="null"/>.</param>
    /// <param name="expirationDate">The UTC expiration date and time for the session.</param>
    /// <returns>A new <see cref="UserSession"/> instance.</returns>
    public static UserSession Create(
        Guid userId,
        string tokenHash,
        string? userAgent,
        string? ip,
        DateTime expirationDate)
    {
        UserSession session = new UserSession();
        session.InitAudit();
        session.UserId = userId;
        session.TokenHash = tokenHash;
        session.UserAgent = userAgent;
        session.Ip = ip;
        session.ExpirationDate = expirationDate;
        return session;
    }

    /// <summary>
    /// Marks this session as revoked, preventing further use.
    /// </summary>
    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
