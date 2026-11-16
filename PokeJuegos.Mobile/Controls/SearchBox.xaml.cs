namespace PokeJuegos.Mobile.Controls;

public partial class SearchBox : ContentView
{
	public static readonly BindableProperty TextProperty =
		BindableProperty.Create(nameof(Text), typeof(string), typeof(SearchBox), string.Empty, BindingMode.TwoWay);

	public static readonly BindableProperty PlaceholderProperty =
		BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(SearchBox), "Search");

	public SearchBox()
	{
		InitializeComponent();
	}

	/// <summary>The user swiped the box sideways; the argument says which way, so it can leave in that direction.</summary>
	public event EventHandler<SwipeDirection>? SwipedAway;

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public string Placeholder
	{
		get => (string)GetValue(PlaceholderProperty);
		set => SetValue(PlaceholderProperty, value);
	}

	/// <summary>Puts the cursor in the field (opens the on-screen keyboard on Android).</summary>
	public void FocusInput() => SearchEntry.Focus();

	/// <summary>Takes the cursor out of the field (hides the on-screen keyboard on Android).</summary>
	public void UnfocusInput() => SearchEntry.Unfocus();

	private void OnSwiped(object? sender, SwipedEventArgs e) => SwipedAway?.Invoke(this, e.Direction);
}
