using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokeJuegos.Mobile.Models;
using PokeJuegos.Mobile.Resources.Strings;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.ViewModels;

/// <summary>
/// User preferences: the app's language (system, English or Spanish).
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
	private readonly IUserConfigService userConfig;
	private readonly INavigationService navigation;

	public SettingsViewModel(IUserConfigService userConfig, INavigationService navigation)
	{
		this.userConfig = userConfig;
		this.navigation = navigation;

		// Language names in their own language, so they can be found whatever the current one is.
		Languages =
		[
			new LanguageOption(null, AppResources.Settings_LanguageSystem),
			new LanguageOption(AppLanguage.English, "English"),
			new LanguageOption(AppLanguage.Spanish, "Español"),
		];
		selectedLanguage = Languages.First(option => option.Code == userConfig.Language);
	}

	public LanguageOption[] Languages { get; }

	// Field-backed so setting it in the constructor doesn't trigger a change.
	private LanguageOption? selectedLanguage;

	public LanguageOption? SelectedLanguage
	{
		get => selectedLanguage;
		set
		{
			if (value is null || !SetProperty(ref selectedLanguage, value) || value.Code == userConfig.Language)
			{
				return;
			}

			// Rebuilds the UI in the new language (this page included) and uploads the change.
			_ = userConfig.SetLanguageAsync(value.Code);
		}
	}

	[RelayCommand]
	private Task GoBackAsync() => navigation.GoBackAsync();
}
