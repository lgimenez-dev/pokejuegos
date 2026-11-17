namespace PokeJuegos.DTO;

/// <summary>
/// The user configuration stored on the server.
/// </summary>
/// <param name="Language"><c>en</c>, <c>es</c>, or <see langword="null"/> to follow the device language.</param>
/// <param name="Sound">Whether sound is on.</param>
/// <param name="UpdatedAt">When the configuration was last changed (UTC); <see langword="null"/> if it was never saved (defaults).</param>
public record UserConfigResponse(
    string? Language,
    bool Sound,
    DateTime? UpdatedAt);
