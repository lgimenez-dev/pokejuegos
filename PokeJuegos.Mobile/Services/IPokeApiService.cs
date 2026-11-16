using PokeJuegos.Mobile.Models;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Pokémon data. Same shape as <c>pokeApi.ts</c> in the web/RN versions, which mirrors PokeAPI:
/// a light list (<c>/pokemon</c>) plus a detail lookup per Pokémon (<c>/pokemon/{name}</c>).
/// Reads the bundled dataset (generations I-III) instead of calling the network, so swapping in
/// a real API later only touches the implementation.
/// </summary>
public interface IPokeApiService
{
	/// <summary>
	/// Gets the list of every Pokémon: name and detail URL only.
	/// </summary>
	/// <returns>The list, ordered by national dex number; loaded once and cached.</returns>
	Task<IReadOnlyList<PokemonSummary>> GetListAsync();

	/// <summary>
	/// Gets the full data of one Pokémon.
	/// </summary>
	/// <param name="name">PokeAPI slug, e.g. <c>mr-mime</c>.</param>
	/// <returns>The Pokémon.</returns>
	/// <exception cref="KeyNotFoundException">No Pokémon with that name in the dataset.</exception>
	Task<Pokemon> GetByNameAsync(string name);
}
