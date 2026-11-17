using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.ViewModels;

/// <summary>Game selector shown after login, with the way to the settings.</summary>
public partial class GeneralViewModel(INavigationService navigation) : ObservableObject
{
	[RelayCommand]
	private Task OpenPokedexAsync() => navigation.GoToAsync(Routes.Pokedex);

	[RelayCommand]
	private Task OpenRouletteAsync() => navigation.GoToAsync(Routes.Roulette);

	[RelayCommand]
	private Task OpenSettingsAsync() => navigation.GoToAsync(Routes.Settings);
}
