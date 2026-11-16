using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace PokeJuegos.Mobile;

// Portrait only: every screen is designed for a vertical phone (31.5rem column, roulette layout).
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		HideStatusBar();
	}

	// Dialogs (the Picker's list, the share sheet) and coming back from another app can bring the bar back.
	public override void OnWindowFocusChanged(bool hasFocus)
	{
		base.OnWindowFocusChanged(hasFocus);
		if (hasFocus)
		{
			HideStatusBar();
		}
	}

	/// <summary>
	/// Hides the status bar; the navigation bar (back/home/recents) stays visible. A swipe from the top edge
	/// shows the status bar for a moment, over the app, and it hides again on its own.
	/// </summary>
	private void HideStatusBar()
	{
		if (Window is null || WindowCompat.GetInsetsController(Window, Window.DecorView) is not { } controller)
		{
			return;
		}

		controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
		controller.Hide(WindowInsetsCompat.Type.StatusBars());
		controller.Show(WindowInsetsCompat.Type.NavigationBars());
	}
}
