using PointBattle.Models;
using SQLite;
using System.Diagnostics;

namespace PointBattle.Services;

public class DatabaseService
{
    SQLiteAsyncConnection Database;
    bool isInitialized = false;
    private readonly object lockObject = new object();

    async Task InitializeAsync()
    {
        if (isInitialized)
            return;

        lock (lockObject)
        {
            if (isInitialized)
                return;
        }

        try
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "pointbattle.db");
            Console.WriteLine($"Database path: {databasePath}");
            
            Database = new SQLiteAsyncConnection(databasePath);

            await Database.CreateTableAsync<Game>();
            await Database.CreateTableAsync<Round>();

            lock (lockObject)
            {
                isInitialized = true;
            }
            
            Console.WriteLine("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }

    public async Task<List<string>> GetAllPlayerNamesAsync()
    {
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting player names: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        try
        {
            await InitializeAsync();
            var games = await Database.Table<Game>().ToListAsync();

            foreach (var game in games)
            {
                try
                {
                    game.Rounds = await Database.Table<Round>()
                        .Where(r => r.GameId == game.Id)
                        .OrderBy(r => r.RoundNumber)
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading rounds for game {game.Id}: {ex.Message}");
                    game.Rounds = new List<Round>();
                }
            }

            return games;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting games: {ex.Message}");
            return new List<Game>();
        }
    }

    public async Task<Game> GetGameAsync(int id)
    {
        try
        {
            await InitializeAsync();
            var game = await Database.Table<Game>().Where(g => g.Id == id).FirstOrDefaultAsync();
            if (game != null)
            {
                try
                {
                    game.Rounds = await Database.Table<Round>()
                        .Where(r => r.GameId == game.Id)
                        .OrderBy(r => r.RoundNumber)
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading rounds for game {id}: {ex.Message}");
                    game.Rounds = new List<Round>();
                }
            }

            return game;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting game {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<Game> GetMostRecentIncompleteGameAsync()
    {
        try
        {
            await InitializeAsync();
            
            Console.WriteLine("Querying for incomplete games...");
            
            var games = await Database.Table<Game>().ToListAsync();
            Console.WriteLine($"Found {games.Count} total games");
            
            var incompleteGames = games.Where(g => !g.IsCompleted).ToList();
            Console.WriteLine($"Found {incompleteGames.Count} incomplete games");
            
            var game = incompleteGames.OrderByDescending(g => g.Date).FirstOrDefault();

            if (game != null)
            {
                Console.WriteLine($"Most recent incomplete game: ID {game.Id}");
                
                try
                {
                    game.Rounds = await Database.Table<Round>()
                        .Where(r => r.GameId == game.Id)
                        .OrderBy(r => r.RoundNumber)
                        .ToListAsync();
                        
                    Console.WriteLine($"Loaded {game.Rounds.Count} rounds for game {game.Id}");
                    
                    // Only return if it actually has rounds
                    if (game.Rounds.Count > 0)
                    {
                        return game;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading rounds for incomplete game: {ex.Message}");
                }
            }

            Console.WriteLine("No recoverable incomplete games found");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting incomplete game: {ex.Message}");
            return null;
        }
    }

    // Get all incomplete games
    public async Task<List<Game>> GetIncompleteGamesAsync()
    {
        try
        {
            await InitializeAsync();
            var games = await Database.Table<Game>()
                .Where(g => !g.IsCompleted)
                .OrderByDescending(g => g.Date)
                .ToListAsync();

            foreach (var game in games)
            {
                try
                {
                    game.Rounds = await Database.Table<Round>()
                        .Where(r => r.GameId == game.Id)
                        .OrderBy(r => r.RoundNumber)
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading rounds for incomplete game {game.Id}: {ex.Message}");
                    game.Rounds = new List<Round>();
                }
            }

            return games;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting incomplete games: {ex.Message}");
            return new List<Game>();
        }
    }

    public async Task<int> SaveGameAsync(Game game)
    {
        try
        {
            await InitializeAsync();

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
                await Database.InsertAsync(game);
                Console.WriteLine($"Created new game with ID: {game.Id}");
                return game.Id;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game: {ex.Message}");
            throw;
        }
    }

    // Enhanced save method with immediate persistence
    public async Task<int> SaveGameImmediatelyAsync(Game game)
    {
        try
        {
            await InitializeAsync();

            // Start a transaction for atomic operation
            await Database.RunInTransactionAsync((SQLiteConnection tran) =>
            {
                if (game.Id != 0)
                {
                    // Update existing game
                    tran.Update(game);

                    foreach (var round in game.Rounds)
                    {
                        if (round.Id != 0)
                            tran.Update(round);
                        else
                        {
                            round.GameId = game.Id;
                            tran.Insert(round);
                        }
                    }
                }
                else
                {
                    // Insert new game
                    game.Id = 0;
                    tran.Insert(game);
                }
            });

            Console.WriteLine($"Immediately saved game with ID: {game.Id}");
            return game.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveGameImmediatelyAsync: {ex}");
            throw;
        }
    }

    // Add a round and save immediately
    public async Task<Round> AddRoundImmediatelyAsync(int gameId, int roundNumber, int groupAPoints, int groupBPoints)
    {
        try
        {
            await InitializeAsync();

            var round = new Round
            {
                GameId = gameId,
                RoundNumber = roundNumber,
                GroupAPoints = groupAPoints,
                GroupBPoints = groupBPoints
            };

            await Database.InsertAsync(round);
            Console.WriteLine($"Added round {roundNumber} to game {gameId}");
            return round;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding round: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteGameAsync(int gameId)
    {
        try
        {
            await InitializeAsync();
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

    public async Task DeleteRoundAsync(int roundId)
    {
        try
        {
            await InitializeAsync();
            Console.WriteLine($"Deleting round with ID: {roundId}");
            await Database.DeleteAsync<Round>(roundId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting round: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateRoundAsync(Round round)
    {
        try
        {
            await InitializeAsync();
        
            Console.WriteLine($"Updating round: {round.Id}, Points: {round.GroupAPoints}-{round.GroupBPoints}");
        
            // Simple direct update
            await Database.ExecuteAsync(
                "UPDATE rounds SET GroupAPoints = ?, GroupBPoints = ? WHERE Id = ?",
                round.GroupAPoints, round.GroupBPoints, round.Id);
        
            Console.WriteLine("Round updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating round: {ex.Message}");
            throw;
        }
    }

    public async Task SaveGameWithRoundsAsync(Game game)
    {
        try
        {
            await InitializeAsync();
        
            // Update the game record
            await Database.UpdateAsync(game);
        
            // Update each round in the game
            foreach (var round in game.Rounds)
            {
                await Database.ExecuteAsync(
                    "UPDATE rounds SET GroupAPoints = ?, GroupBPoints = ? WHERE Id = ?",
                    round.GroupAPoints, round.GroupBPoints, round.Id);
            }
        
            Console.WriteLine($"Successfully updated game {game.Id} with all rounds");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveGameWithRoundsAsync: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAllGamesAsync()
    {
        try
        {
            await InitializeAsync();
            await Database.DeleteAllAsync<Round>();
            await Database.DeleteAllAsync<Game>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting all games: {ex.Message}");
            throw;
        }
    }
}