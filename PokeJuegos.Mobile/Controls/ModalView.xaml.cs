using System.Windows.Input;

namespace PokeJuegos.Mobile.Controls;

/// <summary>
/// Modal card over the page. Whatever goes between its tags in XAML becomes the body
/// (the <c>&lt;slot&gt;</c> of the Vue component), e.g.
/// <c>&lt;controls:ModalView Title="…" IsOpen="{Binding …}"&gt;&lt;VerticalStackLayout&gt;…&lt;/controls:ModalView&gt;</c>.
/// Place it last in a page-filling <see cref="Grid"/> so it draws on top.
/// </summary>
[ContentProperty(nameof(Body))]
public partial class ModalView : ContentView
{
	public static readonly BindableProperty TitleProperty =
		BindableProperty.Create(nameof(Title), typeof(string), typeof(ModalView), string.Empty);

	public static readonly BindableProperty IsOpenProperty =
		BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(ModalView), false,
			propertyChanged: (bindable, _, newValue) => ((ModalView)bindable).OnIsOpenChanged((bool)newValue));

	/// <summary>Runs on the close button and on backdrop taps; it should set <see cref="IsOpen"/> to false.</summary>
	public static readonly BindableProperty CloseCommandProperty =
		BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(ModalView));

	public static readonly BindableProperty BodyProperty =
		BindableProperty.Create(nameof(Body), typeof(View), typeof(ModalView),
			propertyChanged: (bindable, _, newValue) => ((ModalView)bindable).BodyHost.Content = (View?)newValue);

	public ModalView()
	{
		InitializeComponent();
	}

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public bool IsOpen
	{
		get => (bool)GetValue(IsOpenProperty);
		set => SetValue(IsOpenProperty, value);
	}

	public ICommand? CloseCommand
	{
		get => (ICommand?)GetValue(CloseCommandProperty);
		set => SetValue(CloseCommandProperty, value);
	}

	public View? Body
	{
		get => (View?)GetValue(BodyProperty);
		set => SetValue(BodyProperty, value);
	}

	private async void OnIsOpenChanged(bool isOpen)
	{
		this.CancelAnimations();
		if (isOpen)
		{
			Opacity = 0;
			IsVisible = true;
			await this.FadeToAsync(1, 150);
		}
		else
		{
			await this.FadeToAsync(0, 120);

			// Reopened while fading out: leave it visible.
			if (!IsOpen)
			{
				IsVisible = false;
			}
		}
	}
}
