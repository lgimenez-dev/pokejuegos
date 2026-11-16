using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using PokeJuegos.Mobile.Services;
using PokeJuegos.Mobile.ViewModels;
using PokeJuegos.Mobile.Views;

namespace PokeJuegos.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		CrashLogger.Register();

		MauiAppBuilder builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Lato-Regular.ttf", "LatoRegular");
				fonts.AddFont("Lato-Bold.ttf", "LatoBold");
				fonts.AddFont("Lato-Black.ttf", "LatoBlack");
				fonts.AddFont("PressStart2P-Regular.ttf", "PressStart2P");
			});

		RemoveNativeEntryChrome();
		RemoveNativePickerChrome();

		// Services: singletons (stateless or app-wide state).
		builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
		builder.Services.AddSingleton<ISessionService, SessionService>();
#if ANDROID
		builder.Services.AddSingleton<IOAuthBrowser, WebAuthenticatorOAuthBrowser>();
#elif WINDOWS
		builder.Services.AddSingleton<IOAuthBrowser, LoopbackOAuthBrowser>();
#endif
		// MockAuthService works without the API running.
		builder.Services.AddSingleton<IAuthService, HttpAuthService>();
		builder.Services.AddSingleton<LocalDatabase>();
		builder.Services.AddSingleton<IPokeApiService, PokeApiService>();
		builder.Services.AddSingleton<IFavoritesService, FavoritesService>();
		builder.Services.AddSingleton<ITopScoresService, TopScoresService>();

		// Pages and ViewModels: transient, a fresh instance per navigation.
		builder.Services.AddTransient<WelcomePage>();
		builder.Services.AddTransient<WelcomeViewModel>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<ForgotPasswordPage>();
		builder.Services.AddTransient<ForgotPasswordViewModel>();
		builder.Services.AddTransient<CompleteProfilePage>();
		builder.Services.AddTransient<CompleteProfileViewModel>();
		builder.Services.AddTransient<GeneralPage>();
		builder.Services.AddTransient<GeneralViewModel>();
		builder.Services.AddTransient<PokedexPage>();
		builder.Services.AddTransient<PokedexViewModel>();
		builder.Services.AddTransient<RoulettePage>();
		builder.Services.AddTransient<RouletteViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	/// <summary>
	/// Strips the platform's own Entry decoration (Android underline, WinUI border) so the
	/// rounded <c>Border</c> around it in <c>FormEntry</c> is the only visible frame.
	/// </summary>
	private static void RemoveNativeEntryChrome()
	{
		EntryHandler.Mapper.AppendToMapping("NoNativeChrome", (handler, _) =>
		{
#if ANDROID
			handler.PlatformView.Background = null;
			handler.PlatformView.SetPadding(0, 0, 0, 0);
#elif WINDOWS
			handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
			handler.PlatformView.Background = null;
#endif
		});
	}

	/// <summary>
	/// Same for the Picker (Android underline, WinUI combo box border), so the rounded <c>Border</c>
	/// in <c>OptionPicker</c> is the only visible frame.
	/// </summary>
	private static void RemoveNativePickerChrome()
	{
		PickerHandler.Mapper.AppendToMapping("NoNativeChrome", (handler, _) =>
		{
#if ANDROID
			handler.PlatformView.Background = null;
			handler.PlatformView.SetPadding(0, 0, 0, 0);
#elif WINDOWS
			handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
			handler.PlatformView.Background = null;
			handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
#endif
		});
	}
}
