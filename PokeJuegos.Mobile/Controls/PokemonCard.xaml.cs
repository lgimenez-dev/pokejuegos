using System.Windows.Input;

namespace PokeJuegos.Mobile.Controls;

/// <summary>
/// One row of the Pokédex list. Both commands receive the row's <see cref="Models.PokemonItem"/>.
/// </summary>
public partial class PokemonCard : ContentView
{
	/// <summary>Replaces the <c>selected</c> event of the Vue card.</summary>
	public static readonly BindableProperty SelectCommandProperty =
		BindableProperty.Create(nameof(SelectCommand), typeof(ICommand), typeof(PokemonCard));

	/// <summary>Replaces the <c>addFavorite</c> event of the Vue card.</summary>
	public static readonly BindableProperty ToggleFavoriteCommandProperty =
		BindableProperty.Create(nameof(ToggleFavoriteCommand), typeof(ICommand), typeof(PokemonCard));

	public PokemonCard()
	{
		InitializeComponent();
	}

	public ICommand? SelectCommand
	{
		get => (ICommand?)GetValue(SelectCommandProperty);
		set => SetValue(SelectCommandProperty, value);
	}

	public ICommand? ToggleFavoriteCommand
	{
		get => (ICommand?)GetValue(ToggleFavoriteCommandProperty);
		set => SetValue(ToggleFavoriteCommandProperty, value);
	}
}
