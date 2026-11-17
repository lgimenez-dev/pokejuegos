namespace PokeJuegos.Domain.Entities;

/// <summary>
/// A user's preferences (1:1 with <see cref="User"/>). The row is created the first time the user saves something;
/// without it the defaults apply. Does not inherit <see cref="AuditableEntity"/>: the key is the user id and
/// <see cref="UpdatedAt"/> is set by the client, because it decides which copy wins when syncing.
/// </summary>
public sealed class UserConfig
{
    /// <summary>Gets the ID of the user these preferences belong to (primary key).</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the language: <c>en</c>, <c>es</c>, or <see langword="null"/> to follow the device language.</summary>
    public string? Language { get; private set; }

    /// <summary>Gets whether sound is on.</summary>
    public bool Sound { get; private set; }

    /// <summary>Gets when the row was created on the server.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Gets when the client last changed the preferences.</summary>
    public DateTime UpdatedAt { get; private set; }

    private UserConfig() { }

    /// <summary>
    /// Creates the preferences of a user.
    /// </summary>
    /// <param name="userId">The ID of the owning user.</param>
    /// <param name="language"><c>en</c>, <c>es</c>, or <see langword="null"/>.</param>
    /// <param name="sound">Whether sound is on.</param>
    /// <param name="updatedAt">When the client made the change (UTC).</param>
    /// <returns>A new <see cref="UserConfig"/> instance.</returns>
    public static UserConfig Create(Guid userId, string? language, bool sound, DateTime updatedAt)
    {
        UserConfig config = new UserConfig();
        config.UserId = userId;
        config.Language = language;
        config.Sound = sound;
        config.CreatedAt = DateTime.UtcNow;
        config.UpdatedAt = updatedAt;
        return config;
    }

    /// <summary>
    /// Replaces the preferences with a newer copy from the client.
    /// </summary>
    /// <param name="language"><c>en</c>, <c>es</c>, or <see langword="null"/>.</param>
    /// <param name="sound">Whether sound is on.</param>
    /// <param name="updatedAt">When the client made the change (UTC).</param>
    public void Apply(string? language, bool sound, DateTime updatedAt)
    {
        Language = language;
        Sound = sound;
        UpdatedAt = updatedAt;
    }
}
