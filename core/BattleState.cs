namespace PasaporteFiller.core;

/// <summary>
/// Represents the current state of a battle
/// </summary>
public class BattleState
{
    public PlayerPokemon? CurrentPlayerPokemon { get; set; }
    public Pokemon? OpponentPokemon { get; set; }
    public int OpponentLevel { get; set; }
    public int OpponentCurrentHP { get; set; }
    public int OpponentMaxHP { get; set; }

    public DateTime BattleStartTime { get; set; }
    public bool IsInBattle { get; set; }

    /// <summary>
    /// Gets opponent's HP as percentage (0.0 to 1.0)
    /// </summary>
    public float OpponentHPPercentage =>
        OpponentMaxHP > 0 ? (float)OpponentCurrentHP / OpponentMaxHP : 0f;

    public BattleState()
    {
        CurrentPlayerPokemon = null;
        OpponentPokemon = null;
        OpponentLevel = 50;
        OpponentCurrentHP = 0;
        OpponentMaxHP = 0;
        BattleStartTime = DateTime.Now;
        IsInBattle = false;
    }

    /// <summary>
    /// Resets the battle state
    /// </summary>
    public void Reset()
    {
        CurrentPlayerPokemon = null;
        OpponentPokemon = null;
        OpponentLevel = 50;
        OpponentCurrentHP = 0;
        OpponentMaxHP = 0;
        IsInBattle = false;
    }
}
