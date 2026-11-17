using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PokeJuegos.DTO;
using PokeJuegos.Mobile.Models;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// <see cref="IUserConfigService"/> over <see cref="Preferences"/> and <c>/api/user-config</c> of <c>PokeJuegos.Api</c>.
/// </summary>
public class UserConfigService(ISessionService session) : IUserConfigService
{
	private const string LanguageKey = "config_language";
	private const string SoundKey = "config_sound";
	private const string UpdatedAtKey = "config_updated_at";

	private readonly HttpClient http = new(new AcceptLanguageHandler())
	{
		BaseAddress = new Uri(ApiSettings.BaseUrl),
		Timeout = TimeSpan.FromSeconds(15),
	};

	/// <inheritdoc />
	// Preferences can't store null: an empty string stands for "follow the device".
	public string? Language => NullIfEmpty(Preferences.Default.Get(LanguageKey, string.Empty));

	/// <summary>Not editable in the app yet; kept so syncing doesn't overwrite the server's value with a guess.</summary>
	private static bool Sound => Preferences.Default.Get(SoundKey, true);

	/// <summary>When the preferences last changed on this device (UTC), or <c>null</c> if they never did.</summary>
	private static DateTime? UpdatedAt =>
		Preferences.Default.ContainsKey(UpdatedAtKey) ? Preferences.Default.Get(UpdatedAtKey, DateTime.MinValue) : null;

	/// <inheritdoc />
	public void ApplySavedLanguage() => AppLanguage.Apply(Language);

	/// <inheritdoc />
	public async Task SetLanguageAsync(string? language)
	{
		Preferences.Default.Set(LanguageKey, language ?? string.Empty);
		Preferences.Default.Set(UpdatedAtKey, DateTime.UtcNow);

		AppLanguage.Apply(language);
		await RebuildUiAsync($"//{Routes.General}/{Routes.Settings}");
		await SyncAsync();
	}

	/// <inheritdoc />
	public async Task SyncAsync()
	{
		string? token = await session.GetTokenAsync();
		if (token is null)
		{
			return;
		}

		try
		{
			// Never changed here: only download. Otherwise upload; the server keeps whichever copy is newer.
			DateTime? localUpdatedAt = UpdatedAt;
			using HttpRequestMessage message = localUpdatedAt is null
				? new HttpRequestMessage(HttpMethod.Get, "api/user-config")
				: new HttpRequestMessage(HttpMethod.Put, "api/user-config")
				{
					Content = JsonContent.Create(
						new UserConfigRequest(Language, Sound, localUpdatedAt.Value.ToUniversalTime()),
						AuthJsonContext.Default.UserConfigRequest),
				};
			message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			using HttpResponseMessage response = await http.SendAsync(message);
			if (!response.IsSuccessStatusCode)
			{
				return;
			}

			UserConfigResponse? server = await response.Content.ReadFromJsonAsync(AuthJsonContext.Default.UserConfigResponse);
			if (server?.UpdatedAt is not { } serverUpdatedAt || serverUpdatedAt.ToUniversalTime() <= localUpdatedAt?.ToUniversalTime())
			{
				// Nothing saved on the server yet, or this device's copy is the newest.
				return;
			}

			Preferences.Default.Set(LanguageKey, server.Language ?? string.Empty);
			Preferences.Default.Set(SoundKey, server.Sound);
			Preferences.Default.Set(UpdatedAtKey, serverUpdatedAt.ToUniversalTime());

			string previous = AppLanguage.Current;
			AppLanguage.Apply(server.Language);
			if (AppLanguage.Current != previous)
			{
				await RebuildUiAsync(route: null);
			}
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
		{
			// Offline or server down: the local copy goes up on the next sync.
		}
	}

	/// <summary>
	/// Pages read their texts when they are built: a new <see cref="AppShell"/> shows them in the current language.
	/// </summary>
	/// <param name="route">Where to go in the new shell, or <c>null</c> to stay on its first page.</param>
	private static Task RebuildUiAsync(string? route) => MainThread.InvokeOnMainThreadAsync(async () =>
	{
		AppShell shell = new();
		Application.Current!.Windows[0].Page = shell;
		if (route is not null)
		{
			await shell.GoToAsync(route);
		}
	});

	private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;
}
