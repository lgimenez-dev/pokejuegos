using PokeJuegos.Mobile.Models;
using SQLite;

namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Shared SQLite connection for local data (favorites and top scores). Opened lazily on first use.
/// </summary>
public class LocalDatabase
{
	private const string FileName = "pokejuegos.db3";

	private readonly SemaphoreSlim initLock = new(1, 1);
	private SQLiteAsyncConnection? connection;

	/// <summary>
	/// Returns the open connection, creating the file and tables the first time.
	/// </summary>
	/// <returns>The shared connection.</returns>
	public async Task<SQLiteAsyncConnection> GetConnectionAsync()
	{
		if (connection is not null)
		{
			return connection;
		}

		await initLock.WaitAsync();
		try
		{
			if (connection is null)
			{
				string path = Path.Combine(FileSystem.AppDataDirectory, FileName);
				SQLiteAsyncConnection newConnection = new(
					path,
					SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
				await newConnection.EnableWriteAheadLoggingAsync();
				await newConnection.CreateTableAsync<FavoritePokemon>();
				await newConnection.CreateTableAsync<TopScoreEntry>();
				connection = newConnection;
			}

			return connection;
		}
		finally
		{
			initLock.Release();
		}
	}
}
