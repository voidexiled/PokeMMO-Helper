namespace PasaporteFiller.core;

/// <summary>
/// Represents a Pokemon that belongs to the player's team
/// Contains all information needed for battle calculations and team management
/// </summary>
public class PlayerPokemon
{
    // Basic Info
    public string Nickname { get; set; }
    public Pokemon BaseData { get; set; }  // Reference to base Pokemon data from API
    public int Level { get; set; }
    public PokemonGender Gender { get; set; }
    
    // Battle Properties
    public Nature Nature { get; set; }
    public Ability Ability { get; set; }
    public Item? HeldItem { get; set; }
    
    // Stats
    public PokemonStats BaseStats { get; set; }  // From species
    public PokemonStats IVs { get; set; }  // Individual Values (0-31)
    public PokemonStats EVs { get; set; }  // Effort Values (0-252, max 510 total)
    public PokemonStats CalculatedStats { get; set; }  // Final calculated stats
    
    // Moveset (maximum 4 moves)
    public List<LearnedMove> Moveset { get; set; }
    
    // Team Position (1-6)
    public int Order { get; set; }
    
    // Current Battle State
    public int CurrentHP { get; set; }
    public List<string> StatusConditions { get; set; }  // Paralysis, Burn, etc.

    public PlayerPokemon()
    {
        Nickname = "";
        BaseData = null!;
        Level = 50;
        Gender = PokemonGender.Genderless;
        Nature = new Nature("Hardy");
        Ability = new Ability("None");
        HeldItem = null;
        
        BaseStats = new PokemonStats();
        IVs = new PokemonStats(31, 31, 31, 31, 31, 31);  // Default perfect IVs
        EVs = new PokemonStats();  // Default no EVs
        CalculatedStats = new PokemonStats();
        
        Moveset = new List<LearnedMove>();
        Order = 1;
        
        CurrentHP = 0;
        StatusConditions = new List<string>();
    }

    public PlayerPokemon(Pokemon baseData, string nickname, int level)
    {
        Nickname = nickname;
        BaseData = baseData;
        Level = level;
        Gender = PokemonGender.Genderless;
        Nature = new Nature("Hardy");
        Ability = new Ability("None");
        HeldItem = null;
        
        BaseStats = new PokemonStats();
        IVs = new PokemonStats(31, 31, 31, 31, 31, 31);
        EVs = new PokemonStats();
        CalculatedStats = new PokemonStats();
        
        Moveset = new List<LearnedMove>();
        Order = 1;
        
        CurrentHP = 0;
        StatusConditions = new List<string>();
    }

    /// <summary>
    /// Adds a move to the moveset (max 4)
    /// </summary>
    public bool AddMove(LearnedMove move)
    {
        if (Moveset.Count >= 4)
            return false;
        
        Moveset.Add(move);
        return true;
    }

    /// <summary>
    /// Removes a move from the moveset
    /// </summary>
    public bool RemoveMove(int index)
    {
        if (index < 0 || index >= Moveset.Count)
            return false;
        
        Moveset.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Replaces a move at a specific index
    /// </summary>
    public bool ReplaceMove(int index, LearnedMove newMove)
    {
        if (index < 0 || index >= Moveset.Count)
            return false;
        
        Moveset[index] = newMove;
        return true;
    }

    /// <summary>
    /// Gets the Pokemon's current HP percentage
    /// </summary>
    public double GetHPPercentage()
    {
        if (CalculatedStats.HP == 0)
            return 0;
        
        return (double)CurrentHP / CalculatedStats.HP;
    }

    /// <summary>
    /// Checks if the Pokemon has fainted
    /// </summary>
    public bool IsFainted()
    {
        return CurrentHP <= 0;
    }

    /// <summary>
    /// Checks if the Pokemon has any status conditions
    /// </summary>
    public bool HasStatusCondition()
    {
        return StatusConditions.Count > 0;
    }

    /// <summary>
    /// Adds a status condition
    /// </summary>
    public void AddStatusCondition(string condition)
    {
        if (!StatusConditions.Contains(condition))
            StatusConditions.Add(condition);
    }

    /// <summary>
    /// Clears all status conditions
    /// </summary>
    public void ClearStatusConditions()
    {
        StatusConditions.Clear();
    }

    /// <summary>
    /// Gets a display name for the Pokemon (nickname or species name)
    /// </summary>
    public string GetDisplayName()
    {
        return string.IsNullOrEmpty(Nickname) ? BaseData?.Name ?? "Unknown" : Nickname;
    }

    public override string ToString()
    {
        return $"{GetDisplayName()} Lv.{Level} ({CurrentHP}/{CalculatedStats.HP} HP)";
    }
}
