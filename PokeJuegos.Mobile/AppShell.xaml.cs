using PokeJuegos.Mobile.Views;

namespace PokeJuegos.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Pushed routes; pages are resolved from DI so their ViewModels get injected.
		Routing.RegisterRoute(Routes.Login, typeof(LoginPage));
		Routing.RegisterRoute(Routes.Register, typeof(RegisterPage));
		Routing.RegisterRoute(Routes.ForgotPassword, typeof(ForgotPasswordPage));
		Routing.RegisterRoute(Routes.CompleteProfile, typeof(CompleteProfilePage));
		Routing.RegisterRoute(Routes.Pokedex, typeof(PokedexPage));
		Routing.RegisterRoute(Routes.Roulette, typeof(RoulettePage));
	}
}
