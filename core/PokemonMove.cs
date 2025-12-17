namespace PasaporteFiller.core;

public class PokemonMove
{
    public string Name { get; set; }
    public int Power { get; set; }
    public int Accuracy { get; set; }
    public int PP { get; set; }
    public PokemonType Type { get; set; }
    
    // Extended properties for battle mechanics
    public MoveDamageClass DamageClass { get; set; }  // Physical, Special, or Status
    public int Priority { get; set; }  // Move priority (-6 to +5, default 0)
    public MoveTarget Target { get; set; }  // Who the move targets
    public string Description { get; set; }  // Move description
    
    public PokemonMove()
    {
        Name = "";
        Power = 0;
        Accuracy = 100;
        PP = 0;
        Type = new();
        DamageClass = MoveDamageClass.Physical;
        Priority = 0;
        Target = MoveTarget.SelectedPokemon;
        Description = "";
    }
    
    /// <summary>
    /// Returns true if this is a damaging move
    /// </summary>
    public bool IsDamagingMove()
    {
        return Power > 0 && DamageClass != MoveDamageClass.Status;
    }
    
    /// <summary>
    /// Returns true if this is a status move
    /// </summary>
    public bool IsStatusMove()
    {
        return DamageClass == MoveDamageClass.Status;
    }
}