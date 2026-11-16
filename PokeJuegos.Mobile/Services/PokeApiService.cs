using System.Text.Json;
using PokeJuegos.Mobile.Models;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// <see cref="IPokeApiService"/> over the files in <c>Resources/Raw</c>: <c>pokemon_list.json</c> for the list
/// and <c>pokemon_details.json</c> for the details. The details file is only read the first time one is asked for.
/// </summary>
public class PokeApiService : IPokeApiService
{
	private const string ListFile = "pokemon_list.json";
	private const string DetailsFile = "pokemon_details.json";

	private Task<IReadOnlyList<PokemonSummary>>? listTask;
	private Task<Dictionary<string, Pokemon>>? detailsTask;

	/// <inheritdoc />
	public Task<IReadOnlyList<PokemonSummary>> GetListAsync()
	{
		// A failed load isn't cached, so the next call retries.
		if (listTask is null || listTask.IsFaulted)
		{
			listTask = LoadListAsync();
		}

		return listTask;
	}

	/// <inheritdoc />
	public async Task<Pokemon> GetByNameAsync(string name)
	{
		if (detailsTask is null || detailsTask.IsFaulted)
		{
			detailsTask = LoadDetailsAsync();
		}

		Dictionary<string, Pokemon> details = await detailsTask;
		return details.TryGetValue(name, out Pokemon? pokemon)
			? pokemon
			: throw new KeyNotFoundException($"Pokémon '{name}' not found in {DetailsFile}.");
	}

	private static async Task<IReadOnlyList<PokemonSummary>> LoadListAsync()
	{
		await using Stream stream = await FileSystem.OpenAppPackageFileAsync(ListFile);
		PokemonListResponse response = await JsonSerializer.DeserializeAsync(stream, PokemonJsonContext.Default.PokemonListResponse)
			?? throw new InvalidDataException($"{ListFile} is empty.");
		return response.Results;
	}

	private static async Task<Dictionary<string, Pokemon>> LoadDetailsAsync()
	{
		await using Stream stream = await FileSystem.OpenAppPackageFileAsync(DetailsFile);
		List<Pokemon> pokemon = await JsonSerializer.DeserializeAsync(stream, PokemonJsonContext.Default.ListPokemon)
			?? throw new InvalidDataException($"{DetailsFile} is empty.");
		return pokemon.ToDictionary(p => p.Name);
	}
}
