using SQLite;

namespace PokeJuegos.Mobile.Models;

/// <summary>SQLite row: one per Pokémon marked as favorite.</summary>
[Table("favorite_pokemon")]
public class FavoritePokemon
{
	[PrimaryKey]
	public int PokemonId { get; set; }
}
