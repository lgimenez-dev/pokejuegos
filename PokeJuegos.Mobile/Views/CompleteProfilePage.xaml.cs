using PokeJuegos.Mobile.ViewModels;

namespace PokeJuegos.Mobile.Views;

public partial class CompleteProfilePage : ContentPage
{
	private readonly CompleteProfileViewModel viewModel;

	public CompleteProfilePage(CompleteProfileViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		viewModel.LoadCommand.Execute(null);
	}

	// Android back = Cancel: sign out instead of leaving a session without a username.
	protected override bool OnBackButtonPressed()
	{
		viewModel.CancelCommand.Execute(null);
		return true;
	}
}
