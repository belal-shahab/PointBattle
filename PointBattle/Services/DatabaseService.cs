using PointBattle.Models;
using SQLite;
using System.Diagnostics;

namespace PointBattle.Services;

public class DatabaseService
{
    SQLiteAsyncConnection Database;
    bool isInitialized = false;

    async Task InitializeAsync()
    {
        if (isInitialized)
            return;

        try
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "pointbattle.db");
            Database = new SQLiteAsyncConnection(databasePath);

            await Database.CreateTableAsync<Game>();
            await Database.CreateTableAsync<Round>();

            isInitialized = true;
            Debug.WriteLine("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        await InitializeAsync();
        var games = await Database.Table<Game>().ToListAsync();

        foreach (var game in games)
        {
            game.Rounds = await Database.Table<Round>()
                .Where(r => r.GameId == game.Id)
                .OrderBy(r => r.RoundNumber)
                .ToListAsync();
        }

        return games;
    }

    public async Task<Game> GetGameAsync(int id)
    {
        await InitializeAsync();
        var game = await Database.Table<Game>().Where(g => g.Id == id).FirstOrDefaultAsync();
        if (game != null)
        {
            game.Rounds = await Database.Table<Round>()
                .Where(r => r.GameId == game.Id)
                .OrderBy(r => r.RoundNumber)
                .ToListAsync();
        }

        return game;
    }

    public async Task<int> SaveGameAsync(Game game)
    {
        await InitializeAsync();
        if (game.Id != 0)
        {
            await Database.UpdateAsync(game);
            foreach (var round in game.Rounds)
            {
                if (round.Id != 0)
                    await Database.UpdateAsync(round);
                else
                {
                    round.GameId = game.Id;
                    await Database.InsertAsync(round);
                }
            }

            return game.Id;
        }
        else
        {
            var id = await Database.InsertAsync(game);
            foreach (var round in game.Rounds)
            {
                round.GameId = id;
                await Database.InsertAsync(round);
            }

            return id;
        }
    }

    public async Task<int> DeleteGameAsync(Game game)
    {
        await InitializeAsync();
        await Database.Table<Round>().Where(r => r.GameId == game.Id).DeleteAsync();
        return await Database.DeleteAsync(game);
    }

    // Add method to delete all games
    public async Task DeleteAllGamesAsync()
    {
        await InitializeAsync();
        await Database.DeleteAllAsync<Round>();
        await Database.DeleteAllAsync<Game>();
    }

// Add method to update a specific round
    public async Task UpdateRoundAsync(Round round)
    {
        await InitializeAsync();
        await Database.UpdateAsync(round);
    }
}