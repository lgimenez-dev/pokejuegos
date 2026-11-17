using PokeJuegos.Mobile.Views;

namespace PokeJuegos.Mobile;

public partial class AppShell : Shell
{
	// Pushed routes; pages are resolved from DI so their ViewModels get injected. Registered once per process:
	// the shell is rebuilt when the language changes (see UserConfigService).
	static AppShell()
	{
		Routing.RegisterRoute(Routes.Login, typeof(LoginPage));
		Routing.RegisterRoute(Routes.Register, typeof(RegisterPage));
		Routing.RegisterRoute(Routes.ForgotPassword, typeof(ForgotPasswordPage));
		Routing.RegisterRoute(Routes.CompleteProfile, typeof(CompleteProfilePage));
		Routing.RegisterRoute(Routes.Pokedex, typeof(PokedexPage));
		Routing.RegisterRoute(Routes.Roulette, typeof(RoulettePage));
		Routing.RegisterRoute(Routes.Settings, typeof(SettingsPage));
	}

	public AppShell()
	{
		InitializeComponent();
	}
}
