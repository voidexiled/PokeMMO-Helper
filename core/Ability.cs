namespace PasaporteFiller.core;

/// <summary>
/// Represents a Pokemon ability
/// </summary>
public class Ability
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Effect { get; set; }  // Short effect description for battle
    
    public Ability(string name, string description = "", string effect = "")
    {
        Name = name;
        Description = description;
        Effect = effect;
    }

    /// <summary>
    /// Common Pokemon abilities - can be extended later
    /// </summary>
    public static List<Ability> GetCommonAbilities()
    {
        return new List<Ability>
        {
            new Ability("Overgrow", "Powers up Grass-type moves when HP is low", "Boosts Grass moves at 1/3 HP or less"),
            new Ability("Blaze", "Powers up Fire-type moves when HP is low", "Boosts Fire moves at 1/3 HP or less"),
            new Ability("Torrent", "Powers up Water-type moves when HP is low", "Boosts Water moves at 1/3 HP or less"),
            new Ability("Swarm", "Powers up Bug-type moves when HP is low", "Boosts Bug moves at 1/3 HP or less"),
            new Ability("Levitate", "Gives full immunity to all Ground-type moves", "Immune to Ground-type moves"),
            new Ability("Intimidate", "Lowers the foe's Attack stat", "Lowers opponent Attack on switch-in"),
            new Ability("Static", "May paralyze on contact", "30% chance to paralyze attacker on contact"),
            new Ability("Synchronize", "Passes on status problems to the foe", "Passes burn, poison, paralysis to opponent"),
            new Ability("Pressure", "Raises foe's PP usage", "Opponent uses 2 PP instead of 1"),
            new Ability("Thick Fat", "Weakens Fire and Ice moves", "Halves damage from Fire and Ice moves"),
            new Ability("Guts", "Boosts Attack if there is a status problem", "1.5x Attack when statused"),
            new Ability("Technician", "Powers up weaker moves", "1.5x power for moves with 60 power or less"),
            new Ability("Adaptability", "Powers up same-type attack bonus", "2x STAB instead of 1.5x"),
            new Ability("Skill Link", "Increases the frequency of multi-strike moves", "Multi-hit moves always hit 5 times"),
            new Ability("Moxie", "Boosts Attack after knocking out any Pokémon", "+1 Attack on KO"),
            new Ability("Drought", "Turns the sunlight harsh when the Pokémon enters a battle", "Summons sunny weather on switch-in"),
            new Ability("Drizzle", "The Pokémon makes it rain when it enters a battle", "Summons rain on switch-in"),
            new Ability("Sand Stream", "The Pokémon summons a sandstorm in battle", "Summons sandstorm on switch-in"),
            new Ability("Snow Warning", "The Pokémon summons a hailstorm in battle", "Summons hail on switch-in")
        };
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Effect) ? Name : $"{Name} - {Effect}";
    }
}
