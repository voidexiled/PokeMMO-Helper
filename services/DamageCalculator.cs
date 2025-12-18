using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// Calculates damage for Pokemon moves using official game formulas
/// </summary>
public static class DamageCalculator
{
    /// <summary>
    /// Calculates damage range for a move from PlayerPokemon to opponent
    /// </summary>
    public static DamageRange CalculateDamage(
        PlayerPokemon attacker,
        Pokemon defender,
        PokemonMove move,
        int defenderLevel,
        BattleConditions? conditions = null)
    {
        if (move.Power == 0)
        {
            // Status move, no damage
            return new DamageRange();
        }

        conditions ??= new BattleConditions();

        // Determine which stats to use based on move damage class
        int attackStat = move.DamageClass == MoveDamageClass.Physical
            ? attacker.CalculatedStats.Attack
            : attacker.CalculatedStats.SpecialAttack;

        int defenseStat = move.DamageClass == MoveDamageClass.Physical
            ? CalculateDefenderStat(defender.BaseStats.Defense, defenderLevel)
            : CalculateDefenderStat(defender.BaseStats.SpecialDefense, defenderLevel);

        // Apply screens
        if (conditions.HasReflect && move.DamageClass == MoveDamageClass.Physical)
            defenseStat = (int)(defenseStat * 2.0);
        if (conditions.HasLightScreen && move.DamageClass == MoveDamageClass.Special)
            defenseStat = (int)(defenseStat * 2.0);

        // Base damage calculation
        double baseDamage = ((2.0 * attacker.Level / 5.0 + 2.0) * move.Power * attackStat / defenseStat) / 50.0 + 2.0;

        // STAB (Same Type Attack Bonus)
        double stab = HasSTAB(attacker.BaseData, move) ? 1.5 : 1.0;

        // Type effectiveness
        double effectiveness = GetTypeEffectiveness(move.Type, defender.Types);

        // Critical hit
        double critical = conditions.IsCritical ? 1.5 : 1.0;

        // Calculate damage range (random multiplier from 0.85 to 1.0)
        int minDamage = (int)Math.Floor(baseDamage * stab * effectiveness * critical * 0.85);
        int maxDamage = (int)Math.Floor(baseDamage * stab * effectiveness * critical * 1.0);

        // Ensure at least 1 damage if move connects
        if (minDamage < 1 && effectiveness > 0) minDamage = 1;
        if (maxDamage < 1 && effectiveness > 0) maxDamage = 1;

        double avgDamage = (minDamage + maxDamage) / 2.0;

        // Calculate opponent's HP for OHKO/2HKO calculations
        int opponentMaxHP = CalculateDefenderStat(defender.BaseStats.HP, defenderLevel, true);

        // OHKO probability (what % of the random rolls will OHKO)
        double ohkoProbability = CalculateKOProbability(minDamage, maxDamage, opponentMaxHP);

        // 2HKO probability (assumes both hits are average damage)
        double twoHKOProbability = (avgDamage * 2 >= opponentMaxHP) ? 1.0 : 0.0;

        return new DamageRange
        {
            MinDamage = minDamage,
            MaxDamage = maxDamage,
            AverageDamage = avgDamage,
            OHKOProbability = ohkoProbability,
            TwoHKOProbability = twoHKOProbability
        };
    }

    /// <summary>
    /// Calculates type effectiveness multiplier
    /// </summary>
    public static double GetTypeEffectiveness(PokemonType? moveType, List<PokemonType> defenderTypes)
    {
        if (moveType == null || defenderTypes == null || defenderTypes.Count == 0)
            return 1.0;

        double effectiveness = 1.0;

        foreach (var defenderType in defenderTypes)
        {
            // Check immunities (0x)
            if (defenderType.NoDamageFrom.Contains(moveType.Name, StringComparer.OrdinalIgnoreCase))
            {
                return 0.0;
            }

            // Check weaknesses (2x)
            if (defenderType.DoubleDamageFrom.Contains(moveType.Name, StringComparer.OrdinalIgnoreCase))
            {
                effectiveness *= 2.0;
            }
            // Check resistances (0.5x)
            else if (defenderType.HalfDamageFrom.Contains(moveType.Name, StringComparer.OrdinalIgnoreCase))
            {
                effectiveness *= 0.5;
            }
        }

        return effectiveness;
    }

    /// <summary>
    /// Checks if attacker gets STAB (Same Type Attack Bonus)
    /// </summary>
    private static bool HasSTAB(Pokemon attacker, PokemonMove move)
    {
        if (move.Type == null) return false;

        return attacker.Types.Any(t =>
            t.Name.Equals(move.Type.Name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Calculates a stat value for defender (assuming max IVs, no EVs for wild/trainer Pokemon)
    /// </summary>
    private static int CalculateDefenderStat(int baseStat, int level, bool isHP = false)
    {
        // Assume 31 IVs, 0 EVs, neutral nature for opponent
        int iv = 31;
        int ev = 0;

        if (isHP)
        {
            return (int)Math.Floor((2.0 * baseStat + iv + ev / 4.0) * level / 100.0) + level + 10;
        }
        else
        {
            return (int)Math.Floor((2.0 * baseStat + iv + ev / 4.0) * level / 100.0) + 5;
        }
    }

    /// <summary>
    /// Calculates probability of KO given damage range and target HP
    /// </summary>
    private static double CalculateKOProbability(int minDamage, int maxDamage, int targetHP)
    {
        if (minDamage >= targetHP)
            return 1.0;  // Always KO

        if (maxDamage < targetHP)
            return 0.0;  // Never KO

        // Calculate how many of the 16 random rolls will KO
        // Random multiplier goes from 0.85 to 1.0 in 16 discrete steps
        int koRolls = 0;
        for (int i = 0; i < 16; i++)
        {
            double multiplier = 0.85 + (i * 0.15 / 15.0);
            double damage = minDamage / 0.85 * multiplier;  // Scale back to get actual damage for this roll
            if (damage >= targetHP)
                koRolls++;
        }

        return koRolls / 16.0;
    }
}
