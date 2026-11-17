using System.Globalization;
using PokeJuegos.Mobile.Resources.Strings;

namespace PokeJuegos.Mobile.Models;

/// <summary>
/// The extra data the detail modal shows, looked up when a row is tapped (<see cref="Pokemon"/>).
/// Values as the original shows them: weight and height raw from PokeAPI, types lowercase joined by ", "
/// (translated through the <c>Type_&lt;name&gt;</c> resources).
/// Name and sprite stay on the row's <see cref="PokemonItem"/>.
/// </summary>
public sealed class PokemonDetail
{
	private PokemonDetail(Pokemon pokemon)
	{
		Types = string.Join(", ", pokemon.Types.OrderBy(slot => slot.Slot).Select(slot => TypeName(slot.Type.Name)));
		Weight = pokemon.Weight.ToString(CultureInfo.InvariantCulture);
		Height = pokemon.Height.ToString(CultureInfo.InvariantCulture);
	}

	public string Types { get; }

	/// <summary>In hectograms, as PokeAPI gives it.</summary>
	public string Weight { get; }

	/// <summary>In decimeters, as PokeAPI gives it.</summary>
	public string Height { get; }

	public static PokemonDetail From(Pokemon pokemon) => new(pokemon);

	private static string TypeName(string type) =>
		AppResources.ResourceManager.GetString("Type_" + type, AppResources.Culture) ?? type;
}
