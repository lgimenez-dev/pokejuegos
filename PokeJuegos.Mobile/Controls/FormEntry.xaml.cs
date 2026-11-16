namespace PokeJuegos.Mobile.Controls;

public partial class FormEntry : ContentView
{
	public static readonly BindableProperty TextProperty =
		BindableProperty.Create(nameof(Text), typeof(string), typeof(FormEntry), string.Empty, BindingMode.TwoWay);

	public static readonly BindableProperty PlaceholderProperty =
		BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(FormEntry), string.Empty);

	public static readonly BindableProperty IsPasswordProperty =
		BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(FormEntry), false);

	// Plain text by default: no auto-capitalization, like autoCapitalize="none" in the RN version.
	public static readonly BindableProperty KeyboardProperty =
		BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(FormEntry), Keyboard.Plain);

	public static readonly BindableProperty ReturnTypeProperty =
		BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(FormEntry), ReturnType.Default);

	/// <summary>Validation message shown under the field, with a red border; empty hides it.</summary>
	public static readonly BindableProperty ErrorTextProperty =
		BindableProperty.Create(nameof(ErrorText), typeof(string), typeof(FormEntry), string.Empty,
			propertyChanged: (bindable, _, _) => ((FormEntry)bindable).OnPropertyChanged(nameof(HasError)));

	public FormEntry()
	{
		InitializeComponent();
	}

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

	public bool IsPassword
	{
		get => (bool)GetValue(IsPasswordProperty);
		set => SetValue(IsPasswordProperty, value);
	}

	public Keyboard Keyboard
	{
		get => (Keyboard)GetValue(KeyboardProperty);
		set => SetValue(KeyboardProperty, value);
	}

	public ReturnType ReturnType
	{
		get => (ReturnType)GetValue(ReturnTypeProperty);
		set => SetValue(ReturnTypeProperty, value);
	}

	public string ErrorText
	{
		get => (string)GetValue(ErrorTextProperty);
		set => SetValue(ErrorTextProperty, value);
	}

	public bool HasError => !string.IsNullOrEmpty(ErrorText);
}
