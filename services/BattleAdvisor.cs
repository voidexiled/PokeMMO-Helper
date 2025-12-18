using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// AI service that provides battle recommendations
/// </summary>
public static class BattleAdvisor
{
    /// <summary>
    /// Recommends the best move to use against an opponent
    /// </summary>
    public static MoveRecommendation RecommendBestMove(
        PlayerPokemon player,
        Pokemon opponent,
        int opponentLevel,
        int opponentCurrentHP,
        int opponentMaxHP)
    {
        if (player.Moveset.Count == 0)
        {
            return new MoveRecommendation
            {
                Reason = "No moves available"
            };
        }

        // Evaluate all moves
        var evaluations = EvaluateAllMoves(player, opponent, opponentLevel, opponentCurrentHP, opponentMaxHP);

        if (evaluations.Count == 0)
        {
            return new MoveRecommendation
            {
                Reason = "No valid moves to evaluate"
            };
        }

        // Sort by score descending
        evaluations.Sort((a, b) => b.Score.CompareTo(a.Score));

        var best = evaluations[0];
        var alternatives = evaluations.Skip(1).ToList();

        // Generate reason string
        string reason = GenerateRecommendationReason(best, opponentCurrentHP, opponentMaxHP);

        return new MoveRecommendation
        {
            RecommendedMove = best.Move,
            EstimatedDamage = best.Damage,
            Effectiveness = best.Effectiveness,
            Score = best.Score,
            Reason = reason,
            Alternatives = alternatives
        };
    }

    /// <summary>
    /// Evaluates all available moves and returns them sorted by score
    /// </summary>
    public static List<MoveEvaluation> EvaluateAllMoves(
        PlayerPokemon player,
        Pokemon opponent,
        int opponentLevel,
        int opponentCurrentHP,
        int opponentMaxHP)
    {
        var evaluations = new List<MoveEvaluation>();

        foreach (var learnedMove in player.Moveset)
        {
            // Skip moves with no PP
            if (!learnedMove.HasPP)
                continue;

            var move = learnedMove.MoveData;

            // Calculate damage
            var damageRange = DamageCalculator.CalculateDamage(
                player,
                opponent,
                move,
                opponentLevel
            );

            // Get type effectiveness
            double effectiveness = DamageCalculator.GetTypeEffectiveness(move.Type, opponent.Types);

            // Calculate score
            double score = CalculateMoveScore(
                damageRange,
                effectiveness,
                learnedMove,
                opponentCurrentHP,
                opponentMaxHP,
                move
            );

            // Generate notes
            string notes = GenerateMoveNotes(damageRange, effectiveness, opponentCurrentHP, opponentMaxHP);

            evaluations.Add(new MoveEvaluation
            {
                Move = learnedMove,
                Damage = damageRange,
                Effectiveness = effectiveness,
                Score = score,
                Notes = notes
            });
        }

        return evaluations;
    }

    /// <summary>
    /// Recommends whether to switch Pokemon and to which one
    /// </summary>
    public static SwitchRecommendation RecommendSwitch(
        BattleState currentState,
        List<PlayerPokemon> team)
    {
        if (currentState.CurrentPlayerPokemon == null || currentState.OpponentPokemon == null)
        {
            return new SwitchRecommendation
            {
                ShouldSwitch = false,
                Reason = "Insufficient battle information"
            };
        }

        var currentPokemon = currentState.CurrentPlayerPokemon;
        var opponent = currentState.OpponentPokemon;

        // Don't switch if current Pokemon is not at risk
        if (currentPokemon.GetHPPercentage() > 0.5)
        {
            return new SwitchRecommendation
            {
                ShouldSwitch = false,
                Reason = "Current Pokemon is healthy"
            };
        }

        // Find best switch candidate
        PlayerPokemon? bestSwitch = null;
        double bestScore = -1;
        string bestReason = "";

        foreach (var pokemon in team)
        {
            // Skip fainted, skip current
            if (pokemon.IsFainted() || pokemon.Order == currentPokemon.Order)
                continue;

            // Calculate resistance score
            double resistanceScore = CalculateResistanceScore(pokemon, opponent);

            if (resistanceScore > bestScore)
            {
                bestScore = resistanceScore;
                bestSwitch = pokemon;
                bestReason = $"{pokemon.GetDisplayName()} resists opponent's attacks better";
            }
        }

        if (bestSwitch != null && bestScore > 1.5)  // Only recommend if significantly better
        {
            return new SwitchRecommendation
            {
                ShouldSwitch = true,
                RecommendedPokemon = bestSwitch,
                Reason = bestReason,
                Confidence = Math.Min(bestScore / 4.0, 1.0)  // Normalize to 0-1
            };
        }

        return new SwitchRecommendation
        {
            ShouldSwitch = false,
            Reason = "No better switch available"
        };
    }

    /// <summary>
    /// Calculates a score for a move (0-100)
    /// </summary>
    private static double CalculateMoveScore(
        DamageRange damage,
        double effectiveness,
        LearnedMove move,
        int opponentCurrentHP,
        int opponentMaxHP,
        PokemonMove moveData)
    {
        double score = 0;

        // Damage component (40 points max)
        double damageRatio = opponentMaxHP > 0 ? damage.AverageDamage / opponentMaxHP : 0;
        score += Math.Min(damageRatio * 100, 40);

        // Effectiveness component (30 points max)
        double effectivenessScore = effectiveness switch
        {
            0 => 0,        // Immune
            0.25 => 5,     // Quarter damage
            0.5 => 10,     // Half damage
            1.0 => 20,     // Normal
            2.0 => 28,     // Super effective
            4.0 => 30,     // Double super effective
            _ => 20
        };
        score += effectivenessScore;

        // OHKO bonus (20 points max)
        score += damage.OHKOProbability * 20;

        // PP availability (5 points)
        double ppRatio = move.MaxPP > 0 ? (double)move.CurrentPP / move.MaxPP : 0;
        score += ppRatio * 5;

        // Accuracy component (5 points)
        score += (moveData.Accuracy / 100.0) * 5;

        // Penalty for status moves in most cases
        if (moveData.DamageClass == MoveDamageClass.Status)
        {
            score *= 0.3;  // Heavy penalty since we're focusing on damage
        }

        return Math.Min(score, 100);
    }

    /// <summary>
    /// Generates human-readable notes for a move evaluation
    /// </summary>
    private static string GenerateMoveNotes(
        DamageRange damage,
        double effectiveness,
        int opponentCurrentHP,
        int opponentMaxHP)
    {
        var notes = new List<string>();

        // Effectiveness note
        if (effectiveness == 0)
            notes.Add("No effect");
        else if (effectiveness >= 4.0)
            notes.Add("4× super effective!");
        else if (effectiveness >= 2.0)
            notes.Add("Super effective");
        else if (effectiveness <= 0.25)
            notes.Add("Not very effective");
        else if (effectiveness <= 0.5)
            notes.Add("Resisted");

        // OHKO note
        if (damage.OHKOProbability >= 0.9)
            notes.Add("OHKO likely");
        else if (damage.OHKOProbability >= 0.5)
            notes.Add("OHKO possible");
        else if (damage.TwoHKOProbability >= 0.9)
            notes.Add("2HKO");

        // Current HP consideration
        if (opponentCurrentHP < opponentMaxHP && damage.MinDamage >= opponentCurrentHP)
            notes.Add("Guaranteed KO");

        return notes.Count > 0 ? string.Join(", ", notes) : "Standard damage";
    }

    /// <summary>
    /// Generates recommendation reason
    /// </summary>
    private static string GenerateRecommendationReason(
        MoveEvaluation best,
        int opponentCurrentHP,
        int opponentMaxHP)
    {
        var reasons = new List<string>();

        if (best.Effectiveness >= 2.0)
            reasons.Add("super effective");

        if (best.Damage.OHKOProbability >= 0.7)
            reasons.Add("likely OHKO");
        else if (opponentCurrentHP < opponentMaxHP && best.Damage.MinDamage >= opponentCurrentHP)
            reasons.Add("guaranteed KO");
        else if (best.Damage.AverageDamage > opponentMaxHP * 0.4)
            reasons.Add("high damage");

        if (reasons.Count == 0)
            return "Best available option";

        return char.ToUpper(reasons[0][0]) + reasons[0].Substring(1) +
               (reasons.Count > 1 ? $", {string.Join(", ", reasons.Skip(1))}" : "");
    }

    /// <summary>
    /// Calculates how well a Pokemon resists the opponent's attacks
    /// </summary>
    private static double CalculateResistanceScore(PlayerPokemon pokemon, Pokemon opponent)
    {
        // Simple resistance calculation based on type matchups
        double totalResistance = 0;
        int typeCount = 0;

        foreach (var opponentType in opponent.Types)
        {
            foreach (var playerType in pokemon.BaseData.Types)
            {
                // Check if opponent's type does reduced damage to player's type
                if (playerType.HalfDamageFrom.Contains(opponentType.Name, StringComparer.OrdinalIgnoreCase))
                {
                    totalResistance += 2.0;  // Resistant
                }
                else if (playerType.NoDamageFrom.Contains(opponentType.Name, StringComparer.OrdinalIgnoreCase))
                {
                    totalResistance += 4.0;  // Immune
                }
                else if (playerType.DoubleDamageFrom.Contains(opponentType.Name, StringComparer.OrdinalIgnoreCase))
                {
                    totalResistance -= 1.0;  // Weak
                }
                else
                {
                    totalResistance += 1.0;  // Neutral
                }

                typeCount++;
            }
        }

        return typeCount > 0 ? totalResistance / typeCount : 1.0;
    }
}
