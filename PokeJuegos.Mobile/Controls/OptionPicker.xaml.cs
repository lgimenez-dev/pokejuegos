using System.Collections;

namespace PokeJuegos.Mobile.Controls;

/// <summary>
/// Select with a placeholder shown while nothing is picked (port of <c>BaseSelect.vue</c>).
/// <c>SelectedItem</c> <see langword="null"/> is the placeholder, like the <c>"0"</c> value on the web.
/// </summary>
public partial class OptionPicker : ContentView
{
	public static readonly BindableProperty ItemsSourceProperty =
		BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(OptionPicker));

	public static readonly BindableProperty SelectedItemProperty =
		BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(OptionPicker), null, BindingMode.TwoWay,
			propertyChanged: (bindable, _, _) => ((OptionPicker)bindable).OnPropertyChanged(nameof(HasNoSelection)));

	public static readonly BindableProperty PlaceholderProperty =
		BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(OptionPicker), string.Empty);

	public OptionPicker()
	{
		InitializeComponent();
	}

	public IList? ItemsSource
	{
		get => (IList?)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	public object? SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	public string Placeholder
	{
		get => (string)GetValue(PlaceholderProperty);
		set => SetValue(PlaceholderProperty, value);
	}

	/// <summary>Text shown for each item, e.g. <c>{Binding Title, x:DataType=models:RoulettePokemon}</c>.</summary>
	public BindingBase? ItemDisplayBinding
	{
		get => InnerPicker.ItemDisplayBinding;
		set => InnerPicker.ItemDisplayBinding = value;
	}

	public bool HasNoSelection => SelectedItem is null;
}
