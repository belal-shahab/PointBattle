using SQLite;

namespace PointBattle.Models;

[Table("games")]
public class Game
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string GroupA { get; set; } = "Group A";
    public string GroupB { get; set; } = "Group B";
    public DateTime Date { get; set; } = DateTime.Now;
    public string Winner { get; set; } = "";
    public bool IsCompleted { get; set; } = false;
    
    [Ignore]
    public List<Round> Rounds { get; set; } = new List<Round>();
    
    [Ignore]
    public int GroupATotal => Rounds.Sum(r => r.GroupAPoints);
    
    [Ignore]
    public int GroupBTotal => Rounds.Sum(r => r.GroupBPoints);
    
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
}
