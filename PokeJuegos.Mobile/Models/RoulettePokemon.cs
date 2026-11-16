namespace PokeJuegos.Mobile.Models;

/// <summary>
/// A Pokémon in the roulette pool (port of <c>RoulettePokemon</c> in <c>types/roulette.ts</c>).
/// </summary>
/// <param name="Id">National dex number.</param>
/// <param name="Name">PokeAPI slug (<c>mr-mime</c>); the guess is checked against it.</param>
/// <param name="Title">Name with the first letter capitalized, as the web shows it (CSS <c>capitalize</c>).</param>
/// <param name="Sprite">Bundled image name (<c>pokemon_25.webp</c>).</param>
public sealed record RoulettePokemon(int Id, string Name, string Title, string Sprite)
{
	/// <exception cref="FormatException">The list entry's URL doesn't end in a number.</exception>
	public static RoulettePokemon From(PokemonSummary summary)
	{
		int id = summary.Id;
		string name = summary.Name;
		return new RoulettePokemon(id, name, char.ToUpperInvariant(name[0]) + name[1..], $"pokemon_{id}.webp");
	}
}
