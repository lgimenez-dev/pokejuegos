namespace PokeJuegos.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// No dark palette is designed yet (same as the RN version): pin the light theme.
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
