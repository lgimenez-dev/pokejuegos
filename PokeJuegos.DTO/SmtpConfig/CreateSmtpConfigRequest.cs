namespace PokeJuegos.DTO;

/// <summary>
/// Payload for creating the application's SMTP configuration.
/// </summary>
public record CreateSmtpConfigRequest(
    string? Host,
    int Port,
    string? Email,
    string? Password,
    string? SenderName,
    bool UseTls);
