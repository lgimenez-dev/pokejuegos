using PokeJuegos.Mobile.Models;
using SQLite;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// <see cref="ITopScoresService"/> backed by the <c>top_score</c> SQLite table. The first time the table is
/// empty it is seeded with the same mock players as the web (<c>topScores.mock.json</c>).
/// </summary>
public class TopScoresService(LocalDatabase database) : ITopScoresService
{
	private const int TopScoresLimit = 10;

	/// <summary>Username of the player on this device (<c>CURRENT_PLAYER_USERNAME</c> on the web).</summary>
	public const string CurrentPlayerUsername = "You";

	private static readonly (string Username, int Points)[] MockScores =
	[
		("ShadowStrike", 9820),
		("LunarWolf", 9475),
		("PixelRanger", 9100),
		("TurboFalcon", 8890),
		("NebulaFox", 8650),
		("IronClaw", 8420),
		("CyberNova", 7995),
		("RogueSpecter", 7740),
		("EchoBlade", 7525),
		("QuantumByte", 7300),
	];

	private bool seeded;

	/// <inheritdoc />
	public async Task<TopScoresSnapshot> GetAsync()
	{
		SQLiteAsyncConnection connection = await GetSeededConnectionAsync();
		List<TopScoreEntry> sorted = await connection.Table<TopScoreEntry>()
			.OrderByDescending(entry => entry.Points)
			.ToListAsync();

		int index = sorted.FindIndex(entry => entry.Username == CurrentPlayerUsername);
		PlayerRank? currentPlayer = index == -1 ? null : new PlayerRank(index + 1, sorted[index].Points);
		return new TopScoresSnapshot(sorted.Take(TopScoresLimit).ToList(), currentPlayer);
	}

	/// <inheritdoc />
	public async Task UploadAsync(int points)
	{
		SQLiteAsyncConnection connection = await GetSeededConnectionAsync();
		await connection.InsertOrReplaceAsync(new TopScoreEntry { Username = CurrentPlayerUsername, Points = points });
	}

	private async Task<SQLiteAsyncConnection> GetSeededConnectionAsync()
	{
		SQLiteAsyncConnection connection = await database.GetConnectionAsync();
		if (!seeded)
		{
			if (await connection.Table<TopScoreEntry>().CountAsync() == 0)
			{
				await connection.InsertAllAsync(MockScores.Select(score =>
					new TopScoreEntry { Username = score.Username, Points = score.Points }));
			}

			seeded = true;
		}

		return connection;
	}
}
