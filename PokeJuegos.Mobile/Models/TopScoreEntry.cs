using SQLite;

namespace PokeJuegos.Mobile.Models;

/// <summary>SQLite row: one per player in the local top scores (port of <c>TopScorer</c>).</summary>
[Table("top_score")]
public class TopScoreEntry
{
	[PrimaryKey]
	public string Username { get; set; } = string.Empty;

	public int Points { get; set; }
}

/// <summary>The current player's position among all saved scores, even outside the visible top.</summary>
public sealed record PlayerRank(int Rank, int Points);

/// <summary>What the top scores modal shows: the visible top and the current player's entry, if any.</summary>
public sealed record TopScoresSnapshot(IReadOnlyList<TopScoreEntry> Top, PlayerRank? CurrentPlayer);

/// <summary>One row of the top scores list, already formatted (<c>#1 ShadowStrike</c> / <c>9,820 pts</c>).</summary>
public sealed record TopScoreRow(string Player, string Points);
