using System.Linq;
namespace PasaporteFiller.core;

public class Pokemon
{
    public string Name { get; set; }
    public List<PokemonType> Types { get; set; }
    public List<PokemonMove> Moves { get; set; }

    // Base stats from PokéAPI (CRITICAL: needed for PlayerPokemon stat calculations)
    public PokemonStats BaseStats { get; set; }

    public List<string> DoubleDamageFrom { get; set; }
    public List<string> HalfDamageFrom { get; set; }
    public List<string> NoDamageFrom { get; set; }

    public List<string> NormalDamageFrom { get; set; }

    public List<string> DoubleDamageTo { get; set; }
    public List<string> HalfDamageTo { get; set; }
    public List<string> NoDamageTo { get; set; }

    public List<string> NormalDamageTo { get; set; }

    // Parameterless constructor for JSON deserialization
    public Pokemon()
    {
        Name = "";
        Types = new List<PokemonType>();
        Moves = new List<PokemonMove>();
        BaseStats = new PokemonStats();
        DoubleDamageFrom = new List<string>();
        HalfDamageFrom = new List<string>();
        NoDamageFrom = new List<string>();
        NormalDamageFrom = new List<string>();
        DoubleDamageTo = new List<string>();
        HalfDamageTo = new List<string>();
        NoDamageTo = new List<string>();
        NormalDamageTo = new List<string>();
    }


    public Pokemon(string name, List<PokemonType> types, List<PokemonMove> moves, PokemonStats baseStats, List<string> doubleDamageFrom, List<string> halfDamageFrom, List<string> noDamageFrom, List<string> doubleDamageTo, List<string> halfDamageTo, List<string> noDamageTo, List<string> pokemonTypes)
    {
        Name = name;
        Types = types;
        Moves = moves;
        BaseStats = baseStats;

        DoubleDamageFrom = doubleDamageFrom;
        HalfDamageFrom = halfDamageFrom;
        NoDamageFrom = noDamageFrom;
        NormalDamageFrom = pokemonTypes.Except(doubleDamageFrom.Concat(HalfDamageFrom).Concat(NoDamageFrom).ToList()).ToList();


        DoubleDamageTo = doubleDamageTo;
        HalfDamageTo = halfDamageTo;
        NoDamageTo = noDamageTo;
        NormalDamageTo = pokemonTypes.Except(doubleDamageTo.Concat(HalfDamageTo).Concat(NoDamageTo).ToList()).ToList();
    }
}