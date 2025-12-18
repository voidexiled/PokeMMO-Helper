namespace PasaporteFiller.core;

/// <summary>
/// Represents a range of possible damage values for a move
/// </summary>
public class DamageRange
{
    public int MinDamage { get; set; }
    public int MaxDamage { get; set; }
    public double AverageDamage { get; set; }

    /// <summary>
    /// Probability of OHKO (One-Hit Knockout) - 0.0 to 1.0
    /// </summary>
    public double OHKOProbability { get; set; }

    /// <summary>
    /// Probability of 2HKO (Two-Hit Knockout) - 0.0 to 1.0
    /// </summary>
    public double TwoHKOProbability { get; set; }

    public DamageRange()
    {
        MinDamage = 0;
        MaxDamage = 0;
        AverageDamage = 0;
        OHKOProbability = 0;
        TwoHKOProbability = 0;
    }

    public override string ToString()
    {
        return $"{MinDamage}-{MaxDamage} (avg: {AverageDamage:F1})";
    }
}
