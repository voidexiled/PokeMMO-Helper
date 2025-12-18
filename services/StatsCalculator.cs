using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// Service for calculating Pokemon stats based on base stats, IVs, EVs, nature, and level
/// Uses the official Pokemon stat calculation formulas
/// </summary>
public static class StatsCalculator
{
    /// <summary>
    /// Calculates all final stats for a Pokemon
    /// </summary>
    /// <param name="baseStats">Base stats of the Pokemon species</param>
    /// <param name="ivs">Individual Values (0-31 for each stat)</param>
    /// <param name="evs">Effort Values (0-252 per stat, max 510 total)</param>
    /// <param name="nature">Pokemon's nature</param>
    /// <param name="level">Pokemon's level (1-100)</param>
    /// <returns>Calculated final stats</returns>
    public static PokemonStats CalculateFinalStats(
        PokemonStats baseStats,
        PokemonStats ivs,
        PokemonStats evs,
        Nature nature,
        int level)
    {
        var finalStats = new PokemonStats
        {
            HP = CalculateHP(baseStats.HP, ivs.HP, evs.HP, level),
            Attack = CalculateStat("attack", baseStats.Attack, ivs.Attack, evs.Attack, nature, level),
            Defense = CalculateStat("defense", baseStats.Defense, ivs.Defense, evs.Defense, nature, level),
            SpecialAttack = CalculateStat("specialattack", baseStats.SpecialAttack, ivs.SpecialAttack, evs.SpecialAttack, nature, level),
            SpecialDefense = CalculateStat("specialdefense", baseStats.SpecialDefense, ivs.SpecialDefense, evs.SpecialDefense, nature, level),
            Speed = CalculateStat("speed", baseStats.Speed, ivs.Speed, evs.Speed, nature, level)
        };

        return finalStats;
    }

    /// <summary>
    /// Calculates HP stat using the official formula
    /// Formula: floor(((2 * BaseStat + IV + floor(EV/4)) * Level) / 100) + Level + 10
    /// </summary>
    public static int CalculateHP(int baseStat, int iv, int ev, int level)
    {
        if (baseStat == 1) // Shedinja special case
            return 1;

        return (int)Math.Floor(((2 * baseStat + iv + Math.Floor(ev / 4.0)) * level) / 100.0) + level + 10;
    }

    /// <summary>
    /// Calculates a non-HP stat using the official formula
    /// Formula: floor((floor(((2 * BaseStat + IV + floor(EV/4)) * Level) / 100) + 5) * NatureMultiplier)
    /// </summary>
    public static int CalculateStat(string statName, int baseStat, int iv, int ev, Nature nature, int level)
    {
        // Base calculation
        double stat = Math.Floor(((2 * baseStat + iv + Math.Floor(ev / 4.0)) * level) / 100.0) + 5;
        
        // Apply nature multiplier
        double natureMultiplier = nature.GetMultiplier(statName);
        stat = Math.Floor(stat * natureMultiplier);
        
        return (int)stat;
    }

    /// <summary>
    /// Validates that EVs are within allowed limits
    /// Each stat: 0-252
    /// Total: 0-510
    /// </summary>
    public static (bool isValid, string error) ValidateEVs(PokemonStats evs)
    {
        // Check individual stat limits
        if (evs.HP > 252) return (false, "HP EVs cannot exceed 252");
        if (evs.Attack > 252) return (false, "Attack EVs cannot exceed 252");
        if (evs.Defense > 252) return (false, "Defense EVs cannot exceed 252");
        if (evs.SpecialAttack > 252) return (false, "Special Attack EVs cannot exceed 252");
        if (evs.SpecialDefense > 252) return (false, "Special Defense EVs cannot exceed 252");
        if (evs.Speed > 252) return (false, "Speed EVs cannot exceed 252");
        
        if (evs.HP < 0) return (false, "HP EVs cannot be negative");
        if (evs.Attack < 0) return (false, "Attack EVs cannot be negative");
        if (evs.Defense < 0) return (false, "Defense EVs cannot be negative");
        if (evs.SpecialAttack < 0) return (false, "Special Attack EVs cannot be negative");
        if (evs.SpecialDefense < 0) return (false, "Special Defense EVs cannot be negative");
        if (evs.Speed < 0) return (false, "Speed EVs cannot be negative");

        // Check total EVs
        int totalEVs = evs.HP + evs.Attack + evs.Defense + evs.SpecialAttack + evs.SpecialDefense + evs.Speed;
        if (totalEVs > 510)
            return (false, $"Total EVs ({totalEVs}) cannot exceed 510");

        return (true, "");
    }

    /// <summary>
    /// Validates that IVs are within allowed limits (0-31 for each stat)
    /// </summary>
    public static (bool isValid, string error) ValidateIVs(PokemonStats ivs)
    {
        if (ivs.HP < 0 || ivs.HP > 31) return (false, "HP IV must be between 0 and 31");
        if (ivs.Attack < 0 || ivs.Attack > 31) return (false, "Attack IV must be between 0 and 31");
        if (ivs.Defense < 0 || ivs.Defense > 31) return (false, "Defense IV must be between 0 and 31");
        if (ivs.SpecialAttack < 0 || ivs.SpecialAttack > 31) return (false, "Special Attack IV must be between 0 and 31");
        if (ivs.SpecialDefense < 0 || ivs.SpecialDefense > 31) return (false, "Special Defense IV must be between 0 and 31");
        if (ivs.Speed < 0 || ivs.Speed > 31) return (false, "Speed IV must be between 0 and 31");

        return (true, "");
    }

    /// <summary>
    /// Gets the total EVs invested
    /// </summary>
    public static int GetTotalEVs(PokemonStats evs)
    {
        return evs.HP + evs.Attack + evs.Defense + evs.SpecialAttack + evs.SpecialDefense + evs.Speed;
    }

    /// <summary>
    /// Calculates stats and returns validation result
    /// </summary>
    public static (bool isValid, PokemonStats? stats, string error) CalculateAndValidate(
        PokemonStats baseStats,
        PokemonStats ivs,
        PokemonStats evs,
        Nature nature,
        int level)
    {
        // Validate IVs
        var ivValidation = ValidateIVs(ivs);
        if (!ivValidation.isValid)
            return (false, null, ivValidation.error);

        // Validate EVs
        var evValidation = ValidateEVs(evs);
        if (!evValidation.isValid)
            return (false, null, evValidation.error);

        // Validate level
        if (level < 1 || level > 100)
            return (false, null, "Level must be between 1 and 100");

        // Calculate stats
        var finalStats = CalculateFinalStats(baseStats, ivs, evs, nature, level);
        return (true, finalStats, "");
    }
}
