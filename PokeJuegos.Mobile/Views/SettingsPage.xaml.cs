using PokeJuegos.Mobile.ViewModels;

namespace PokeJuegos.Mobile.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
