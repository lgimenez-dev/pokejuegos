using PokeJuegos.Mobile.ViewModels;

namespace PokeJuegos.Mobile.Views;

public partial class WelcomePage : ContentPage
{
	public WelcomePage(WelcomeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
