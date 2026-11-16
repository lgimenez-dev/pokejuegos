namespace PokeJuegos.Domain.Entities;

/// <summary>
/// Represents the SMTP configuration used for sending emails. The application maintains a single record.
/// </summary>
public sealed class SmtpConfig : AuditableEntity
{
    /// <summary>Gets the SMTP server hostname or IP address.</summary>
    public string Host { get; private set; } = string.Empty;

    /// <summary>Gets the SMTP server port.</summary>
    public int Port { get; private set; }

    /// <summary>Gets the sender email address used for outgoing messages.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Gets the SMTP account password.</summary>
    public string Password { get; private set; } = string.Empty;

    /// <summary>Gets the display name shown as the sender in outgoing emails.</summary>
    public string SenderName { get; private set; } = string.Empty;

    /// <summary>Gets a value indicating whether TLS is enabled for the SMTP connection.</summary>
    public bool UseTls { get; private set; }

    private SmtpConfig() { }

    /// <summary>
    /// Creates a new <see cref="SmtpConfig"/> with the specified settings.
    /// </summary>
    /// <param name="host">The SMTP server hostname or IP address.</param>
    /// <param name="port">The SMTP server port.</param>
    /// <param name="email">The sender email address.</param>
    /// <param name="password">The SMTP account password.</param>
    /// <param name="senderName">The display name for outgoing emails.</param>
    /// <param name="useTls">Whether to enable TLS.</param>
    /// <returns>A new <see cref="SmtpConfig"/> instance.</returns>
    public static SmtpConfig Create(
        string host,
        int port,
        string email,
        string password,
        string senderName,
        bool useTls)
    {
        SmtpConfig config = new SmtpConfig();
        config.InitAudit();
        config.Host = host;
        config.Port = port;
        config.Email = email;
        config.Password = password;
        config.SenderName = senderName;
        config.UseTls = useTls;
        return config;
    }

    /// <summary>
    /// Replaces all mutable SMTP settings with the provided values.
    /// </summary>
    /// <param name="host">The new SMTP server hostname or IP address.</param>
    /// <param name="port">The new SMTP server port.</param>
    /// <param name="email">The new sender email address.</param>
    /// <param name="password">The new SMTP account password.</param>
    /// <param name="senderName">The new display name for outgoing emails.</param>
    /// <param name="useTls">Whether to enable TLS.</param>
    public void Update(
        string host,
        int port,
        string email,
        string password,
        string senderName,
        bool useTls)
    {
        Host = host;
        Port = port;
        Email = email;
        Password = password;
        SenderName = senderName;
        UseTls = useTls;
        SetUpdatedAt();
    }
}
