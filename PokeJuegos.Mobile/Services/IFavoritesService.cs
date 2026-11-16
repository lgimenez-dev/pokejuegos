namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Favorite Pokémon, persisted across app restarts.
/// </summary>
public interface IFavoritesService
{
	/// <summary>
	/// Gets the ids of every favorite Pokémon.
	/// </summary>
	/// <returns>The favorite ids.</returns>
	Task<IReadOnlySet<int>> GetFavoriteIdsAsync();

	/// <summary>
	/// Adds the Pokémon to favorites, or removes it if it already was one.
	/// </summary>
	/// <param name="pokemonId">National dex number.</param>
	/// <returns><see langword="true"/> if it is a favorite after the call.</returns>
	Task<bool> ToggleAsync(int pokemonId);
}
