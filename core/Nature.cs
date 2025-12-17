namespace PasaporteFiller.core;

/// <summary>
/// Represents a Pokemon nature with stat modifiers
/// </summary>
public class Nature
{
    public string Name { get; set; }
    public string IncreasedStat { get; set; }  // Stat that gets +10% (1.1x multiplier)
    public string DecreasedStat { get; set; }  // Stat that gets -10% (0.9x multiplier)

    public Nature(string name, string increasedStat = "", string decreasedStat = "")
    {
        Name = name;
        IncreasedStat = increasedStat;
        DecreasedStat = decreasedStat;
    }

    /// <summary>
    /// Returns true if this nature is neutral (doesn't modify stats)
    /// </summary>
    public bool IsNeutral()
    {
        return string.IsNullOrEmpty(IncreasedStat) || string.IsNullOrEmpty(DecreasedStat);
    }

    /// <summary>
    /// Gets the multiplier for a given stat
    /// </summary>
    public double GetMultiplier(string statName)
    {
        statName = statName.ToLower();
        
        if (statName == "hp")
            return 1.0; // HP is never affected by nature

        if (statName == IncreasedStat.ToLower())
            return 1.1;
        
        if (statName == DecreasedStat.ToLower())
            return 0.9;
        
        return 1.0;
    }

    /// <summary>
    /// Gets all available natures
    /// </summary>
    public static List<Nature> GetAllNatures()
    {
        return new List<Nature>
        {
            // Neutral natures
            new Nature("Hardy"),
            new Nature("Docile"),
            new Nature("Serious"),
            new Nature("Bashful"),
            new Nature("Quirky"),
            
            // Attack boosting
            new Nature("Lonely", "attack", "defense"),
            new Nature("Brave", "attack", "speed"),
            new Nature("Adamant", "attack", "specialattack"),
            new Nature("Naughty", "attack", "specialdefense"),
            
            // Defense boosting
            new Nature("Bold", "defense", "attack"),
            new Nature("Relaxed", "defense", "speed"),
            new Nature("Impish", "defense", "specialattack"),
            new Nature("Lax", "defense", "specialdefense"),
            
            // Special Attack boosting
            new Nature("Modest", "specialattack", "attack"),
            new Nature("Mild", "specialattack", "defense"),
            new Nature("Quiet", "specialattack", "speed"),
            new Nature("Rash", "specialattack", "specialdefense"),
            
            // Special Defense boosting
            new Nature("Calm", "specialdefense", "attack"),
            new Nature("Gentle", "specialdefense", "defense"),
            new Nature("Sassy", "specialdefense", "speed"),
            new Nature("Careful", "specialdefense", "specialattack"),
            
            // Speed boosting
            new Nature("Timid", "speed", "attack"),
            new Nature("Hasty", "speed", "defense"),
            new Nature("Jolly", "speed", "specialattack"),
            new Nature("Naive", "speed", "specialdefense")
        };
    }

    /// <summary>
    /// Gets a nature by name
    /// </summary>
    public static Nature? GetNature(string name)
    {
        return GetAllNatures().FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public override string ToString()
    {
        if (IsNeutral())
            return $"{Name} (Neutral)";
        
        return $"{Name} (+{IncreasedStat}, -{DecreasedStat})";
    }
}
