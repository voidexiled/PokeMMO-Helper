using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// Represents evaluation of a single move
/// </summary>
public class MoveEvaluation
{
    public LearnedMove Move { get; set; }
    public DamageRange Damage { get; set; }
    public double Effectiveness { get; set; }
    public double Score { get; set; }  // 0-100
    public string Notes { get; set; }

    public MoveEvaluation()
    {
        Move = null!;
        Damage = new DamageRange();
        Effectiveness = 1.0;
        Score = 0;
        Notes = "";
    }
}

/// <summary>
/// Represents a move recommendation result
/// </summary>
public class MoveRecommendation
{
    public LearnedMove RecommendedMove { get; set; }
    public DamageRange EstimatedDamage { get; set; }
    public double Effectiveness { get; set; }
    public double Score { get; set; }
    public string Reason { get; set; }
    public List<MoveEvaluation> Alternatives { get; set; }

    public MoveRecommendation()
    {
        RecommendedMove = null!;
        EstimatedDamage = new DamageRange();
        Effectiveness = 1.0;
        Score = 0;
        Reason = "";
        Alternatives = new List<MoveEvaluation>();
    }
}

/// <summary>
/// Represents a Pokemon switch recommendation
/// </summary>
public class SwitchRecommendation
{
    public bool ShouldSwitch { get; set; }
    public PlayerPokemon? RecommendedPokemon { get; set; }
    public string Reason { get; set; }
    public double Confidence { get; set; }  // 0.0 - 1.0

    public SwitchRecommendation()
    {
        ShouldSwitch = false;
        RecommendedPokemon = null;
        Reason = "";
        Confidence = 0;
    }
}
