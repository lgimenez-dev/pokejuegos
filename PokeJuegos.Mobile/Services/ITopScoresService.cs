using PokeJuegos.Mobile.Models;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Local roulette top scores, persisted across app restarts (port of <c>useTopScoresStore.ts</c>).
/// </summary>
public interface ITopScoresService
{
	/// <summary>
	/// Gets the visible top and the current player's rank among every saved score.
	/// </summary>
	/// <returns>The top 10, highest first, and the current player's entry if they ever uploaded.</returns>
	Task<TopScoresSnapshot> GetAsync();

	/// <summary>
	/// Saves the current player's score, replacing their previous one.
	/// </summary>
	/// <param name="points">Score to save.</param>
	/// <returns>A task that completes when the score is saved.</returns>
	Task UploadAsync(int points);
}
