using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile;

public partial class App : Application
{
	private readonly IUserConfigService userConfig;

	public App(IUserConfigService userConfig)
	{
		this.userConfig = userConfig;
		InitializeComponent();

		// No dark palette is designed yet (same as the RN version): pin the light theme.
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Texts are read when pages are built: the language goes first.
		userConfig.ApplySavedLanguage();

		Window window = new(new AppShell());
		window.Created += async (_, _) => await userConfig.SyncAsync();
		return window;
	}
}
