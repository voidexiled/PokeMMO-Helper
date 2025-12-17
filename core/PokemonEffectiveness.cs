namespace PasaporteFiller.core;

public class PokemonEffectiveness
{
    public List<string> DoubleDamageFrom { get; set; }
    public List<string> HalfDamageFrom { get; set; }
    public List<string> NoDamageFrom { get; set; }
    
    public List<string> DoubleDamageTo { get; set; }
    
    public List<string> HalfDamageTo { get; set; }
    
    public List<string> NoDamageTo { get; set; }
    

    public PokemonEffectiveness()
    {
        DoubleDamageFrom = new List<string>();
        HalfDamageFrom = new List<string>();
        NoDamageFrom = new List<string>();
        DoubleDamageTo = new List<string>();
        HalfDamageTo = new List<string>();
        NoDamageTo = new List<string>();
    }
    public PokemonEffectiveness(List<string> doubleDamageFrom, List<string> halfDamageFrom, List<string> noDamageFrom, List<string> doubleDamageTo, List<string> halfDamageTo, List<string> noDamageTo)
    {
        DoubleDamageFrom = doubleDamageFrom;
        HalfDamageFrom = halfDamageFrom;
        NoDamageFrom = noDamageFrom;
        DoubleDamageTo = doubleDamageTo;
        HalfDamageTo = halfDamageTo;
        NoDamageTo = noDamageTo;
    }
}