using CommunityToolkit.Mvvm.ComponentModel;

namespace PokeJuegos.Mobile.Models;

/// <summary>
/// One row of the Pokédex list, built from the light list entry (<see cref="PokemonSummary"/>).
/// The list shows the raw name, like the original; the favorite flag is observed by the card and the detail modal.
/// </summary>
public partial class PokemonItem : ObservableObject
{
	private PokemonItem(int id, string name, bool isFavorite)
	{
		Id = id;
		Name = name;
		Title = char.ToUpperInvariant(name[0]) + name[1..];
		Sprite = $"pokemon_{id}.webp";
		IsFavorite = isFavorite;
	}

	/// <summary>National dex number; key for favorites and the sprite.</summary>
	public int Id { get; }

	/// <summary>PokeAPI slug (<c>bulbasaur</c>, <c>mr-mime</c>): shown as-is in the list and used by search and the detail lookup.</summary>
	public string Name { get; }

	/// <summary>Name with the first letter capitalized, for the detail modal title (<c>capitalizedName</c> in the Vue modal).</summary>
	public string Title { get; }

	/// <summary>Bundled image name (<c>Resources/Images/Sprites</c>), usable directly as an <see cref="Image.Source"/>.</summary>
	public string Sprite { get; }

	[ObservableProperty]
	public partial bool IsFavorite { get; set; }

	/// <summary>
	/// Builds the row from a list entry (the id comes from <see cref="PokemonSummary.Id"/>).
	/// </summary>
	/// <exception cref="FormatException">The URL doesn't end in a number.</exception>
	public static PokemonItem From(PokemonSummary summary, Func<int, bool> isFavorite)
	{
		int id = summary.Id;
		return new PokemonItem(id, summary.Name, isFavorite(id));
	}

	/// <summary>
	/// Same rule as the web/RN versions: the query matches the start of the name or of any of its words.
	/// </summary>
	public bool Matches(string query) =>
		Name.StartsWith(query, StringComparison.OrdinalIgnoreCase)
		|| Name.Split('-').Any(word => word.StartsWith(query, StringComparison.OrdinalIgnoreCase));
}
