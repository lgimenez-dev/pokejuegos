using System.Windows.Input;
using PokeJuegos.Mobile.Resources.Strings;

namespace PokeJuegos.Mobile.Controls;

public partial class ErrorView : ContentView
{
	public static readonly BindableProperty ErrorMessageProperty =
		BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(ErrorView), string.Empty);

	public static readonly BindableProperty ButtonTextProperty =
		BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(ErrorView), string.Empty,
			defaultValueCreator: _ => AppResources.Common_GoBackHome);

	/// <summary>Replaces the <c>backToHome</c> event of the Vue/RN component.</summary>
	public static readonly BindableProperty BackToHomeCommandProperty =
		BindableProperty.Create(nameof(BackToHomeCommand), typeof(ICommand), typeof(ErrorView));

	public ErrorView()
	{
		InitializeComponent();
	}

	public string ErrorMessage
	{
		get => (string)GetValue(ErrorMessageProperty);
		set => SetValue(ErrorMessageProperty, value);
	}

	public string ButtonText
	{
		get => (string)GetValue(ButtonTextProperty);
		set => SetValue(ButtonTextProperty, value);
	}

	public ICommand? BackToHomeCommand
	{
		get => (ICommand?)GetValue(BackToHomeCommandProperty);
		set => SetValue(BackToHomeCommandProperty, value);
	}
}
