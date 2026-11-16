using System.Windows.Input;

namespace PokeJuegos.Mobile.Controls;

/// <summary>
/// Yes/No confirmation over the page (port of <c>BaseConfirmModal.vue</c>). Like <see cref="ModalView"/>,
/// place it last in a page-filling <see cref="Grid"/>. Both commands should set <see cref="IsOpen"/> to false.
/// </summary>
public partial class ConfirmModalView : ContentView
{
	public static readonly BindableProperty TitleProperty =
		BindableProperty.Create(nameof(Title), typeof(string), typeof(ConfirmModalView), string.Empty);

	public static readonly BindableProperty MessageProperty =
		BindableProperty.Create(nameof(Message), typeof(string), typeof(ConfirmModalView), string.Empty);

	public static readonly BindableProperty IsOpenProperty =
		BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(ConfirmModalView), false);

	public static readonly BindableProperty ConfirmLabelProperty =
		BindableProperty.Create(nameof(ConfirmLabel), typeof(string), typeof(ConfirmModalView), "Yes");

	public static readonly BindableProperty CancelLabelProperty =
		BindableProperty.Create(nameof(CancelLabel), typeof(string), typeof(ConfirmModalView), "No");

	/// <summary>Replaces the <c>confirm</c> event of the Vue component.</summary>
	public static readonly BindableProperty ConfirmCommandProperty =
		BindableProperty.Create(nameof(ConfirmCommand), typeof(ICommand), typeof(ConfirmModalView));

	/// <summary>Runs on "No", the close button and backdrop taps.</summary>
	public static readonly BindableProperty CancelCommandProperty =
		BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(ConfirmModalView));

	public ConfirmModalView()
	{
		InitializeComponent();
	}

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public string Message
	{
		get => (string)GetValue(MessageProperty);
		set => SetValue(MessageProperty, value);
	}

	public bool IsOpen
	{
		get => (bool)GetValue(IsOpenProperty);
		set => SetValue(IsOpenProperty, value);
	}

	public string ConfirmLabel
	{
		get => (string)GetValue(ConfirmLabelProperty);
		set => SetValue(ConfirmLabelProperty, value);
	}

	public string CancelLabel
	{
		get => (string)GetValue(CancelLabelProperty);
		set => SetValue(CancelLabelProperty, value);
	}

	public ICommand? ConfirmCommand
	{
		get => (ICommand?)GetValue(ConfirmCommandProperty);
		set => SetValue(ConfirmCommandProperty, value);
	}

	public ICommand? CancelCommand
	{
		get => (ICommand?)GetValue(CancelCommandProperty);
		set => SetValue(CancelCommandProperty, value);
	}
}
