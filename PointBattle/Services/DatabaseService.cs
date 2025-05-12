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
    public async Task<List<string>> GetAllPlayerNamesAsync()
    {
        await InitializeAsync();
    
        var games = await GetGamesAsync();
        var names = new HashSet<string>();
    
        foreach (var game in games)
        {
            if (!string.IsNullOrEmpty(game.GroupA))
                names.Add(game.GroupA);
            
            if (!string.IsNullOrEmpty(game.GroupAMember2))
                names.Add(game.GroupAMember2);
            
            if (!string.IsNullOrEmpty(game.GroupB))
                names.Add(game.GroupB);
            
            if (!string.IsNullOrEmpty(game.GroupBMember2))
                names.Add(game.GroupBMember2);
        }
    
        return names.Where(n => !string.IsNullOrEmpty(n))
            .OrderBy(n => n)
            .ToList();
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

        try
        {
            if (game.Id != 0)
            {
                // Updating existing game
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

                Console.WriteLine($"Updated game with ID: {game.Id}");
                return game.Id;
            }
            else
            {
                // Force ID to be 0 for a new game
                game.Id = 0;

                // Create a completely new game
                await Database.InsertAsync(game);

                Console.WriteLine($"Created new game with ID: {game.Id}");

                // No rounds yet for a brand new game
                return game.Id;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveGameAsync: {ex}");
            throw;
        }
    }
    public async Task DeleteGameAsync(int gameId)
    {
        await InitializeAsync();
        try
        {
            Console.WriteLine($"Deleting game with ID: {gameId}");

            // First delete all rounds for this game
            await Database.Table<Round>().Where(r => r.GameId == gameId).DeleteAsync();

            // Then delete the game itself
            await Database.DeleteAsync<Game>(gameId);

            Console.WriteLine($"Successfully deleted game with ID: {gameId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting game: {ex.Message}");
            throw;
        }
    }
   

// Add method to delete a specific round
    public async Task DeleteRoundAsync(int roundId)
    {
        await InitializeAsync();
        try
        {
            Console.WriteLine($"Deleting round with ID: {roundId}");
            await Database.DeleteAsync<Round>(roundId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting round: {ex}");
            throw;
        }
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
    
        try 
        {
            Console.WriteLine($"Updating round: {round.Id}, Points: {round.GroupAPoints}-{round.GroupBPoints}");
        
            // Simple direct update
            await Database.ExecuteAsync(
                "UPDATE rounds SET GroupAPoints = ?, GroupBPoints = ? WHERE Id = ?",
                round.GroupAPoints, round.GroupBPoints, round.Id);
        
            Console.WriteLine("Round updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateRoundAsync: {ex.Message}");
            throw;
        }
    }
}