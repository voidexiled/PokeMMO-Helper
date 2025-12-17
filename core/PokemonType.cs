namespace PasaporteFiller.core;

public class PokemonType
{
    public string Name { get; set; }
    
    // Debilidades
    public List<string> DoubleDamageFrom { get; set; }
    public List<string> HalfDamageFrom { get; set; }
    public List<string> NoDamageFrom { get; set; }
    
    //Fortalezas
    public List<string> DoubleDamageTo { get; set; }
    public List<string> HalfDamageTo { get; set; }
    public List<string> NoDamageTo { get; set; }
    
    
    public PokemonType(string name, List<string> doubleDamageFrom, List<string> halfDamageFrom, List<string> noDamageFrom, List<string> doubleDamageTo, List<string> halfDamageTo, List<string> noDamageTo)
    {
        Name = name;
        
        DoubleDamageFrom = doubleDamageFrom;
        HalfDamageFrom = halfDamageFrom;
        NoDamageFrom = noDamageFrom;
        DoubleDamageTo = doubleDamageTo;
        HalfDamageTo = halfDamageTo;
        NoDamageTo = noDamageTo;
    }

    
}