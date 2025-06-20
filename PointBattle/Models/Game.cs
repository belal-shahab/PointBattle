using SQLite;

namespace PointBattle.Models;

[Table("games")]
public class Game
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string GroupA { get; set; } = "Group A";
    public string GroupAMember2 { get; set; } = "";

    public string GroupB { get; set; } = "Group B";
    public string GroupBMember2 { get; set; } = "";

    public DateTime Date { get; set; } = DateTime.Now;
    public string Winner { get; set; } = "";
    public bool IsCompleted { get; set; } = false;
    
    // NEW: Last updated timestamp for recovery
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    
    // NEW: Flag to track if game was recovered
    public bool WasRecovered { get; set; } = false;
    
    [Ignore]
    public List<Round> Rounds { get; set; } = new List<Round>();
    
    [Ignore]
    public int GroupATotal => Rounds.Sum(r => r.GroupAPoints);
    
    [Ignore]
    public int GroupBTotal => Rounds.Sum(r => r.GroupBPoints);
    
    [Ignore]
    public string GroupAFullName => string.IsNullOrEmpty(GroupAMember2) ? 
        GroupA : $"{GroupA} & {GroupAMember2}";
        
    [Ignore]
    public string GroupBFullName => string.IsNullOrEmpty(GroupBMember2) ? 
        GroupB : $"{GroupB} & {GroupBMember2}";
    
    public const int MaxRounds = 8;  // Set maximum rounds to 8
    
    public const int MaxPointsPerRound = 400;  // Maximum points per round
    
    // Calculate if Group B needs more than achievable points to win
    public bool IsGroupAWinConfirmed()
    {
        int remainingRounds = MaxRounds - Rounds.Count;
        int maxPossiblePointsForB = remainingRounds * MaxPointsPerRound;
        int pointsNeededForB = GroupATotal - GroupBTotal + 1;
        
        return pointsNeededForB > maxPossiblePointsForB;
    }
    
    // Calculate if Group A needs more than achievable points to win
    public bool IsGroupBWinConfirmed()
    {
        int remainingRounds = MaxRounds - Rounds.Count;
        int maxPossiblePointsForA = remainingRounds * MaxPointsPerRound;
        int pointsNeededForA = GroupBTotal - GroupATotal + 1;
        
        return pointsNeededForA > maxPossiblePointsForA;
    }
    
    // Points needed for Group B to win
    public int PointsNeededForGroupB()
    {
        return GroupATotal - GroupBTotal + 1;
    }
    
    // Points needed for Group A to win
    public int PointsNeededForGroupA()
    {
        return GroupBTotal - GroupATotal + 1;
    }

    // NEW: Check if game is recoverable (has rounds but not completed)
    [Ignore]
    public bool IsRecoverable => Rounds.Count > 0 && !IsCompleted;

    // NEW: Get recovery info string
    [Ignore]
    public string RecoveryInfo => IsRecoverable ? 
        $"Round {Rounds.Count}/{MaxRounds} - {GroupATotal}:{GroupBTotal}" : "";
}

[Table("rounds")]
public class Round
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
        
    [Indexed]
    public int GameId { get; set; }
    public int RoundNumber { get; set; }
    public int GroupAPoints { get; set; }
    public int GroupBPoints { get; set; }
    
    // NEW: Timestamp when round was added
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}