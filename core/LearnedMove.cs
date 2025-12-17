namespace PasaporteFiller.core;

/// <summary>
/// Represents a move that has been learned by a player's Pokemon
/// Wraps PokemonMove with current PP tracking
/// </summary>
public class LearnedMove
{
    public PokemonMove MoveData { get; set; }
    public int CurrentPP { get; set; }

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
    /// Gets the maximum PP for this move
    /// </summary>
    public int MaxPP => MoveData.PP;

    /// <summary>
    /// Gets the PP percentage (0-1)
    /// </summary>
    public double PPPercentage => (double)CurrentPP / MaxPP;

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
