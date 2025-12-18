namespace PasaporteFiller.core;

/// <summary>
/// Represents a move that has been learned by a player's Pokemon
/// Wraps PokemonMove with current PP tracking
/// </summary>
public class LearnedMove
{
    public PokemonMove MoveData { get; set; }
    public int CurrentPP { get; set; }

    // Parameterless constructor for JSON deserialization
    public LearnedMove()
    {
        MoveData = null!;
        CurrentPP = 0;
    }

    public LearnedMove(PokemonMove moveData)
    {
        MoveData = moveData;
        CurrentPP = moveData.PP;  // Initialize with max PP
    }

    public LearnedMove(PokemonMove moveData, int currentPP)
    {
        MoveData = moveData;
        CurrentPP = currentPP;
    }

    /// <summary>
    /// Gets the maximum PP for this move (null-safe)
    /// </summary>
    public int MaxPP => MoveData?.PP ?? 0;

    /// <summary>
    /// Gets the PP percentage (0-1) with division by zero protection
    /// </summary>
    public double PPPercentage => MaxPP > 0 ? (double)CurrentPP / MaxPP : 0;

    /// <summary>
    /// Checks if the move has PP remaining
    /// </summary>
    public bool HasPP => CurrentPP > 0;

    /// <summary>
    /// Uses the move (decrements PP)
    /// </summary>
    public bool UseMove()
    {
        if (CurrentPP > 0)
        {
            CurrentPP--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Restores PP by a certain amount
    /// </summary>
    public void RestorePP(int amount)
    {
        CurrentPP = Math.Min(CurrentPP + amount, MaxPP);
    }

    /// <summary>
    /// Fully restores PP
    /// </summary>
    public void FullRestorePP()
    {
        CurrentPP = MaxPP;
    }

    public override string ToString()
    {
        return $"{MoveData.Name} ({CurrentPP}/{MaxPP} PP)";
    }
}
