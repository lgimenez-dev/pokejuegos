namespace PokeJuegos.DTO;

/// <summary>
/// The whole user configuration as the client has it, sent to sync it with the server (last write wins).
/// </summary>
/// <param name="Language"><c>en</c>, <c>es</c>, or <see langword="null"/> to follow the device language.</param>
/// <param name="Sound">Whether sound is on.</param>
/// <param name="UpdatedAt">When the client last changed the configuration (UTC). Decides which copy wins.</param>
public record UserConfigRequest(
    string? Language,
    bool Sound,
    DateTime UpdatedAt);
