using Newtonsoft.Json.Linq;

namespace PasaporteFiller.core;

public class Pokemon
{
    public string Name { get; set; }
    public List<PokemonType> Types { get; set; }
    public List<PokemonMove> Moves { get; set; }
    
    public List<string> DoubleDamageFrom { get; set; }
    public List<string> HalfDamageFrom { get; set; }
    public List<string> NoDamageFrom { get; set; }
    
    public List<string> NormalDamageFrom { get; set; }
    
    public List<string> DoubleDamageTo { get; set; }
    public List<string> HalfDamageTo { get; set; }
    public List<string> NoDamageTo { get; set; }
    
    public List<string> NormalDamageTo { get; set; }

    
    public Pokemon(string name, List<PokemonType> types, List<PokemonMove> moves, List<string> doubleDamageFrom, List<string> halfDamageFrom, List<string> noDamageFrom, List<string> doubleDamageTo, List<string> halfDamageTo, List<string> noDamageTo, List<string> pokemonTypes)
    {
        Name = name;
        Types = types;
        Moves = moves;
        
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