using PokeJuegos.Mobile.ViewModels;

namespace PokeJuegos.Mobile.Views;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
