using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.ViewModels;

public partial class WelcomeViewModel(INavigationService navigation) : ObservableObject
{
	[RelayCommand]
	private Task GetStartedAsync() => navigation.GoToAsync(Routes.Login);
}
