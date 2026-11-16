namespace PokeJuegos.DTO;

/// <summary>
/// Represents an active session returned by the session management endpoints.
/// </summary>
public record UserSessionResponse(
    Guid Id,
    Guid UserId,
    string? UserAgent,
    string? Ip,
    DateTime ExpirationDate,
    DateTime CreatedAt);
