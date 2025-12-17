namespace PasaporteFiller.core;

/// <summary>
/// Represents a Pokemon's stats (HP, Attack, Defense, Special Attack, Special Defense, Speed)
/// Can be used for base stats, IVs, EVs, or calculated stats
/// </summary>
public class PokemonStats
{
    public int HP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int SpecialAttack { get; set; }
    public int SpecialDefense { get; set; }
    public int Speed { get; set; }

    public PokemonStats()
    {
        HP = 0;
        Attack = 0;
        Defense = 0;
        SpecialAttack = 0;
        SpecialDefense = 0;
        Speed = 0;
    }

    public PokemonStats(int hp, int attack, int defense, int specialAttack, int specialDefense, int speed)
    {
        HP = hp;
        Attack = attack;
        Defense = defense;
        SpecialAttack = specialAttack;
        SpecialDefense = specialDefense;
        Speed = speed;
    }

    /// <summary>
    /// Creates a copy of this stats object
    /// </summary>
    public PokemonStats Clone()
    {
        return new PokemonStats(HP, Attack, Defense, SpecialAttack, SpecialDefense, Speed);
    }

    /// <summary>
    /// Gets the stat value by name
    /// </summary>
    public int GetStat(string statName)
    {
        return statName.ToLower() switch
        {
            "hp" => HP,
            "attack" or "atk" => Attack,
            "defense" or "def" => Defense,
            "specialattack" or "spatk" or "spa" => SpecialAttack,
            "specialdefense" or "spdef" or "spd" => SpecialDefense,
            "speed" or "spe" => Speed,
            _ => 0
        };
    }

    /// <summary>
    /// Sets the stat value by name
    /// </summary>
    public void SetStat(string statName, int value)
    {
        switch (statName.ToLower())
        {
            case "hp":
                HP = value;
                break;
            case "attack" or "atk":
                Attack = value;
                break;
            case "defense" or "def":
                Defense = value;
                break;
            case "specialattack" or "spatk" or "spa":
                SpecialAttack = value;
                break;
            case "specialdefense" or "spdef" or "spd":
                SpecialDefense = value;
                break;
            case "speed" or "spe":
                Speed = value;
                break;
        }
    }

    public override string ToString()
    {
        return $"HP: {HP}, Atk: {Attack}, Def: {Defense}, SpA: {SpecialAttack}, SpD: {SpecialDefense}, Spe: {Speed}";
    }
}
