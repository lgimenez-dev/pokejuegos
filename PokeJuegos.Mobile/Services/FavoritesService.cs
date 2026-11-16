using PokeJuegos.Mobile.Models;
using SQLite;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// <see cref="IFavoritesService"/> backed by the <c>favorite_pokemon</c> SQLite table.
/// </summary>
public class FavoritesService(LocalDatabase database) : IFavoritesService
{
	/// <inheritdoc />
	public async Task<IReadOnlySet<int>> GetFavoriteIdsAsync()
	{
		SQLiteAsyncConnection connection = await database.GetConnectionAsync();
		List<FavoritePokemon> rows = await connection.Table<FavoritePokemon>().ToListAsync();
		return rows.Select(row => row.PokemonId).ToHashSet();
	}

	/// <inheritdoc />
	public async Task<bool> ToggleAsync(int pokemonId)
	{
		SQLiteAsyncConnection connection = await database.GetConnectionAsync();
		int deleted = await connection.DeleteAsync<FavoritePokemon>(pokemonId);
		if (deleted > 0)
		{
			return false;
		}

		await connection.InsertAsync(new FavoritePokemon { PokemonId = pokemonId });
		return true;
	}
}
