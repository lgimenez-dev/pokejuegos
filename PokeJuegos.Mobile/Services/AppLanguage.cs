using System.Globalization;
using PokeJuegos.Mobile.Resources.Strings;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// The language the UI is shown in: English or Spanish. <c>null</c> as the preference means "follow the device":
/// Spanish if the device is in any Spanish variant, English otherwise.
/// </summary>
public static class AppLanguage
{
	public const string English = "en";
	public const string Spanish = "es";

	/// <summary>The effective language (<c>en</c> or <c>es</c>), also sent as <c>Accept-Language</c> to the API.</summary>
	public static string Current { get; private set; } = English;

	/// <summary>
	/// Sets the culture the texts are read in. Only the UI culture changes: numbers and dates keep the device's format.
	/// Pages already built keep their texts; recreate the <see cref="AppShell"/> to see the change.
	/// </summary>
	/// <param name="preference"><c>en</c>, <c>es</c>, or <c>null</c> to follow the device.</param>
	public static void Apply(string? preference)
	{
		Current = preference ?? (CultureInfo.InstalledUICulture.TwoLetterISOLanguageName == Spanish ? Spanish : English);

		CultureInfo culture = new(Current);
		AppResources.Culture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;
		CultureInfo.CurrentUICulture = culture;
	}
}
