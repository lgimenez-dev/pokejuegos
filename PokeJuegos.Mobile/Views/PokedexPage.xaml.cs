using System.ComponentModel;
using PokeJuegos.Mobile.ViewModels;

namespace PokeJuegos.Mobile.Views;

public partial class PokedexPage : ContentPage
{
	private const uint SearchAnimationMs = 200;

	private readonly PokedexViewModel viewModel;

	// Side the search field leaves through when it closes: the swipe's direction, or right by default.
	private SwipeDirection searchExitDirection = SwipeDirection.Right;

	public PokedexPage(PokedexViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
		viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		viewModel.LoadCommand.Execute(null);
	}

	// Android back button: close the detail modal, then the search field, before leaving the page.
	protected override bool OnBackButtonPressed() => DismissTopLayer() || base.OnBackButtonPressed();

	/// <summary>Closes the detail modal or, if it isn't open, the search field.</summary>
	/// <returns><see langword="true"/> if something was closed.</returns>
	private bool DismissTopLayer()
	{
		if (viewModel.IsDetailOpen)
		{
			viewModel.CloseDetailCommand.Execute(null);
			return true;
		}

		if (viewModel.IsSearchOpen)
		{
			viewModel.CloseSearchCommand.Execute(null);
			return true;
		}

		return false;
	}

	private void OnSearchSwipedAway(object? sender, SwipeDirection direction)
	{
		searchExitDirection = direction;
		viewModel.CloseSearchCommand.Execute(null);
	}

	private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(PokedexViewModel.IsSearchOpen))
		{
			return;
		}

		HeaderButtons.CancelAnimations();
		SearchPanel.CancelAnimations();

		if (viewModel.IsSearchOpen)
		{
			// Slide in from the magnifier's side while the arrow and magnifier fade out.
			SearchPanel.TranslationX = 40;
			SearchPanel.Opacity = 0;
			SearchPanel.IsVisible = true;
			await Task.WhenAll(
				SearchPanel.TranslateToAsync(0, 0, SearchAnimationMs, Easing.CubicOut),
				SearchPanel.FadeToAsync(1, SearchAnimationMs),
				HeaderButtons.FadeToAsync(0, SearchAnimationMs));
			HeaderButtons.IsVisible = !viewModel.IsSearchOpen;
			SearchPanel.FocusInput();
		}
		else
		{
			SearchPanel.UnfocusInput();
			double exitX = searchExitDirection == SwipeDirection.Left ? -SearchPanel.Width : SearchPanel.Width;
			searchExitDirection = SwipeDirection.Right;
			HeaderButtons.IsVisible = true;
			await Task.WhenAll(
				SearchPanel.TranslateToAsync(exitX, 0, SearchAnimationMs, Easing.CubicIn),
				SearchPanel.FadeToAsync(0, SearchAnimationMs),
				HeaderButtons.FadeToAsync(1, SearchAnimationMs));
			SearchPanel.IsVisible = viewModel.IsSearchOpen;
		}
	}

#if WINDOWS
	// Escape closes the modal or the search field. Handled at page level (with handledEventsToo, since
	// the TextBox may mark the key as handled) so it works wherever the keyboard focus is on the page.
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
		if (e.Key == Windows.System.VirtualKey.Escape && DismissTopLayer())
		{
			e.Handled = true;
		}
	}
#endif
}
