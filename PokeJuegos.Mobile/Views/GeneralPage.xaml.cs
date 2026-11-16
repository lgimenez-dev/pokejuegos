using PokeJuegos.Mobile.ViewModels;

namespace PokeJuegos.Mobile.Views;

public partial class GeneralPage : ContentPage
{
	public GeneralPage(GeneralViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
