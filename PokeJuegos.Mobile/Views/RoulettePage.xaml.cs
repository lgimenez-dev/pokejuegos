using System.ComponentModel;
using PokeJuegos.Mobile.Controls;
using PokeJuegos.Mobile.ViewModels;

namespace PokeJuegos.Mobile.Views;

public partial class RoulettePage : ContentPage
{
	private const string PulseAnimation = "GameOverPulse";

	// .bottomCard.shake keyframes (0.5 s): translateX at every 10%.
	private static readonly double[] ShakeSteps = [-2, 4, -8, 8, -8, 8, -8, 4, -2, 0];

	private readonly RouletteViewModel viewModel;

	// IsFuseVisible is re-raised with the rest of the round state; only react when it actually flips.
	private bool isFuseBurning;

	public RoulettePage(RouletteViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
		viewModel.PropertyChanged += OnViewModelPropertyChanged;

		// Same offset the burst uses for its center (margin-top: -8vh on the web).
		Stage.SizeChanged += (_, _) => SilhouetteWrapper.TranslationY = BurstBackground.CenterOffsetRatio * Stage.Height;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		viewModel.LoadCommand.Execute(null);
	}

	// Leaving mid-spin must not leave timers running (BACKLOG 4.11).
	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		viewModel.Stop();
		Fuse.Stop();
		isFuseBurning = false;
		Confetti.Stop();
		StopPulse();
	}

	// Android back button: close the open modal, otherwise ask before leaving (like the back arrow).
	protected override bool OnBackButtonPressed()
	{
		DismissTopLayerOrAskToLeave();
		return true;
	}

	private void DismissTopLayerOrAskToLeave()
	{
		if (viewModel.IsLeaveConfirmOpen)
		{
			viewModel.CancelLeaveCommand.Execute(null);
		}
		else if (viewModel.IsTopScoresOpen)
		{
			viewModel.CloseTopScoresCommand.Execute(null);
		}
		else
		{
			viewModel.RequestLeaveCommand.Execute(null);
		}
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(RouletteViewModel.IsFuseVisible) when viewModel.IsFuseVisible != isFuseBurning:
				isFuseBurning = viewModel.IsFuseVisible;
				if (isFuseBurning)
				{
					Fuse.Start(viewModel.FuseDurationMs);
				}
				else
				{
					Fuse.Stop();
				}

				break;

			// useGuessFeedback: confetti from the silhouette on a hit; shake on a miss or a burnt-out fuse.
			case nameof(RouletteViewModel.GuessResult) when viewModel.GuessResult == GuessResult.Correct:
				Confetti.Burst(new Point(Stage.Width / 2, Stage.Height * (0.5 + BurstBackground.CenterOffsetRatio)));
				break;

			case nameof(RouletteViewModel.GuessResult) when viewModel.GuessResult == GuessResult.Incorrect:
				_ = ShakeCardAsync();
				break;

			case nameof(RouletteViewModel.IsGameOver):
				if (viewModel.IsGameOver)
				{
					StartPulse();
				}
				else
				{
					StopPulse();
				}

				break;
		}
	}

	private async Task ShakeCardAsync()
	{
		BottomCard.CancelAnimations();
		foreach (double x in ShakeSteps)
		{
			await BottomCard.TranslateToAsync(x, 0, 50);
		}
	}

	// gameOverPulse: scale 1 → 1.06 → 1 every second, forever.
	private void StartPulse()
	{
		new Animation
		{
			{ 0, 0.5, new Animation(scale => GameOverBanner.Scale = scale, 1, 1.06, Easing.SinInOut) },
			{ 0.5, 1, new Animation(scale => GameOverBanner.Scale = scale, 1.06, 1, Easing.SinInOut) },
		}.Commit(GameOverBanner, PulseAnimation, length: 1000, repeat: () => true);
	}

	private void StopPulse()
	{
		GameOverBanner.AbortAnimation(PulseAnimation);
		GameOverBanner.Scale = 1;
	}

#if WINDOWS
	// Escape does what the Android back button does. Handled at page level (with handledEventsToo) so it
	// works wherever the keyboard focus is on the page.
	private Microsoft.UI.Xaml.UIElement? keyboardSource;

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();
		Microsoft.UI.Xaml.Input.KeyEventHandler handler = OnPlatformKeyDown;
		keyboardSource?.RemoveHandler(Microsoft.UI.Xaml.UIElement.KeyDownEvent, handler);
		keyboardSource = Handler?.PlatformView as Microsoft.UI.Xaml.UIElement;
		keyboardSource?.AddHandler(Microsoft.UI.Xaml.UIElement.KeyDownEvent, handler, handledEventsToo: true);
	}

	private void OnPlatformKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		if (e.Key == Windows.System.VirtualKey.Escape)
		{
			DismissTopLayerOrAskToLeave();
			e.Handled = true;
		}
	}
#endif
}
