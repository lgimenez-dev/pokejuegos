namespace PokeJuegos.DTO;

/// <summary>
/// Payload for replacing all fields of the existing SMTP configuration.
/// </summary>
public record UpdateSmtpConfigRequest(
    string? Host,
    int Port,
    string? Email,
    string? Password,
    string? SenderName,
    bool UseTls);
