namespace PokeJuegos.Mobile.Services;

/// <summary>
/// The user's preferences (language for now), kept on the device and synced with <c>/api/user-config</c>
/// when there is a session. The most recent change wins.
/// </summary>
public interface IUserConfigService
{
	/// <summary>The chosen language: <c>en</c>, <c>es</c>, or <c>null</c> to follow the device.</summary>
	string? Language { get; }

	/// <summary>
	/// Applies the saved language to the UI culture. Call before building the first page.
	/// </summary>
	void ApplySavedLanguage();

	/// <summary>
	/// Saves a new language, rebuilds the UI in it (back on the settings page) and uploads it if there is a session.
	/// </summary>
	/// <param name="language"><c>en</c>, <c>es</c>, or <c>null</c> to follow the device.</param>
	/// <returns>A task that completes when the UI is rebuilt and the sync attempt finished.</returns>
	Task SetLanguageAsync(string? language);

	/// <summary>
	/// Syncs with the server if there is a session: uploads the local copy (the server keeps the newest) and applies
	/// the server's copy when it is newer, rebuilding the UI if the language changed. Never throws: without a connection
	/// the local copy is uploaded on the next sync.
	/// </summary>
	/// <returns>A task that completes when the sync attempt finished.</returns>
	Task SyncAsync();
}
