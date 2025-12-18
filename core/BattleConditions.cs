namespace PasaporteFiller.core;

/// <summary>
/// Represents battle conditions that affect damage calculations
/// </summary>
public class BattleConditions
{
    public string? Weather { get; set; }
    public bool IsCritical { get; set; }
    public bool HasReflect { get; set; }  // Screens that reduce physical damage
    public bool HasLightScreen { get; set; }  // Screens that reduce special damage

    public BattleConditions()
    {
        Weather = null;
        IsCritical = false;
        HasReflect = false;
        HasLightScreen = false;
    }
}
