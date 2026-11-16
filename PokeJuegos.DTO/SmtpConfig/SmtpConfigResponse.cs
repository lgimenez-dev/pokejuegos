namespace PokeJuegos.DTO;

/// <summary>
/// Represents the current SMTP configuration returned by the API. Password is intentionally omitted.
/// </summary>
public record SmtpConfigResponse(
    Guid Id,
    string Host,
    int Port,
    string Email,
    string SenderName,
    bool UseTls);
