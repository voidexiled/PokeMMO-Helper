using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PasaporteFiller.core;

namespace PasaporteFiller.services;

public static class PokemonService
{
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

    private static string POKEMON_API_URL = "https://pokeapi.co/api/v2/pokemon/";
    private static string POKEMON_TYPE_API_URL = "https://pokeapi.co/api/v2/type/";
    private static string POKEMON_MOVE_API_URL = "https://pokeapi.co/api/v2/move/";

    private static string POKEMON_IMAGE_URL = "https://raw.githubusercontent.com/duiker101/pokemon-type-svg-icons/master/icons/";

    private static readonly Dictionary<string, PokemonMove> _moveCache = new Dictionary<string, PokemonMove>();

    public static async Task<List<String>> GetPokemonList()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{POKEMON_API_URL}?limit=1500");
            var json = JObject.Parse(response);
            var pokemonList = json["results"].Select(p => p["name"].ToString()).Where(name => !name.Contains("-")).ToList();
            return pokemonList;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return [];
        }
    }
    public static async Task<Pokemon?> GetPokemon(string name)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{POKEMON_API_URL}{name.ToLower()}");
            var json = JObject.Parse(response);
            //var types = json["types"].Select(t => t["type"]["name"].ToString()).ToList();
            //var moves = json["moves"].Select(m => m["move"]["name"].ToString()).ToList();
            var pokemonTypes = new List<PokemonType>();
            var pokemonMoves = new List<PokemonMove>();
            var typesNames = json["types"].Select(t => t["type"]["name"].ToString()).ToList();

            // Load moves from API
            var movesArray = json["moves"];
            if (movesArray != null)
            {
                foreach (var moveObj in movesArray)
                {
                    string moveName = moveObj["move"]?["name"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(moveName))
                    {
                        // Format move name (replace hyphens with spaces, capitalize first letter)
                        moveName = char.ToUpper(moveName[0]) + moveName.Substring(1).Replace("-", " ");
                        pokemonMoves.Add(new PokemonMove { Name = moveName, Power = 0, PP = 10 });
                    }
                }
            }

            foreach (var typeName in typesNames)
            {
                //var imageUrl = $"{POKEMON_IMAGE_URL}{typeName}.svg";

                var weaknessesAndStrengths = await GetTypeWeaknessesAndStrengths(typeName);

                var doubleDamageFrom = weaknessesAndStrengths[0];
                var halfDamageFrom = weaknessesAndStrengths[1];
                var noDamageFrom = weaknessesAndStrengths[2];
                var doubleDamageTo = weaknessesAndStrengths[3];
                var halfDamageTo = weaknessesAndStrengths[4];
                var noDamageTo = weaknessesAndStrengths[5];


                pokemonTypes.Add(
                    new PokemonType(
                        typeName,
                        doubleDamageFrom,
                        halfDamageFrom,
                        noDamageFrom,
                        doubleDamageTo,
                        halfDamageTo,
                        noDamageTo
                    )
                );
            }

            // Extract base stats from PokéAPI
            var baseStats = new PokemonStats();
            var statsArray = json["stats"];
            if (statsArray != null)
            {
                foreach (var statObj in statsArray)
                {
                    string statName = statObj["stat"]?["name"]?.ToString() ?? "";
                    int baseStat = (int)(statObj["base_stat"] ?? 0);

                    switch (statName)
                    {
                        case "hp":
                            baseStats.HP = baseStat;
                            break;
                        case "attack":
                            baseStats.Attack = baseStat;
                            break;
                        case "defense":
                            baseStats.Defense = baseStat;
                            break;
                        case "special-attack":
                            baseStats.SpecialAttack = baseStat;
                            break;
                        case "special-defense":
                            baseStats.SpecialDefense = baseStat;
                            break;
                        case "speed":
                            baseStats.Speed = baseStat;
                            break;
                    }
                }
            }

            PokemonEffectiveness CombinedEffectiveness;

            if (pokemonTypes.Count > 1)
            {
                CombinedEffectiveness = CalculateCombinedEffectiveness(pokemonTypes[0], pokemonTypes[1]);
            }
            else
            {
                CombinedEffectiveness = CalculateCombinedEffectiveness(pokemonTypes[0]);
            }

            var allTypes = await GetAllTypes();
            Console.WriteLine(JObject.FromObject(CombinedEffectiveness));
            var pokemon = new Pokemon(
                name,
                pokemonTypes,
                pokemonMoves,
                baseStats,  // ⭐ NEW: Pass base stats to constructor
                CombinedEffectiveness.DoubleDamageFrom,
                CombinedEffectiveness.HalfDamageFrom,
                CombinedEffectiveness.NoDamageFrom,
                CombinedEffectiveness.DoubleDamageTo,
                CombinedEffectiveness.HalfDamageTo,
                CombinedEffectiveness.NoDamageTo,
                allTypes

            );
            return pokemon;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public static async Task<List<List<string>>> GetTypeWeaknessesAndStrengths(string typeName)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{POKEMON_TYPE_API_URL}{typeName.ToLower()}");
            var json = JObject.Parse(response);
            var doubleDamageFrom = json["damage_relations"]["double_damage_from"].Select(t => t["name"].ToString()).ToList();
            var halfDamageFrom = json["damage_relations"]["half_damage_from"].Select(t => t["name"].ToString()).ToList();
            var noDamageFrom = json["damage_relations"]["no_damage_from"].Select(t => t["name"].ToString()).ToList();
            var doubleDamageTo = json["damage_relations"]["double_damage_to"].Select(t => t["name"].ToString()).ToList();
            var halfDamageTo = json["damage_relations"]["half_damage_to"].Select(t => t["name"].ToString()).ToList();
            var noDamageTo = json["damage_relations"]["no_damage_to"].Select(t => t["name"].ToString()).ToList();
            return new List<List<string>> { doubleDamageFrom, halfDamageFrom, noDamageFrom, doubleDamageTo, halfDamageTo, noDamageTo };
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new List<List<string>>();

        }
    }

    public static PokemonEffectiveness CalculateEffectiveness(PokemonType type1, PokemonType? type2 = null)
    {
        var effectivenessFromDict = new Dictionary<string, double>();
        var effectivenessToDict = new Dictionary<string, double>();


        void UpdateEffectivenessFrom(List<string> types, double multiplier)
        {
            foreach (var type in types)
            {
                if (effectivenessFromDict.ContainsKey(type))
                {
                    effectivenessFromDict[type] *= multiplier;
                }
                else
                {
                    effectivenessFromDict[type] = multiplier;
                }
            }
        }
        void UpdateEffectivenessTo(List<string> types, double multiplier)
        {
            foreach (var type in types)
            {
                if (effectivenessToDict.ContainsKey(type))
                {
                    effectivenessToDict[type] *= multiplier;
                }
                else
                {
                    effectivenessToDict[type] = multiplier;
                }
            }
        }

        // Update effectiveness based on type1
        UpdateEffectivenessFrom(type1.DoubleDamageFrom, 2.0);
        UpdateEffectivenessFrom(type1.HalfDamageFrom, 0.5);
        UpdateEffectivenessFrom(type1.NoDamageFrom, 0.0);

        if (type2 != null)
        {
            // Update effectiveness based on type2
            UpdateEffectivenessFrom(type2.DoubleDamageFrom, 2.0);
            UpdateEffectivenessFrom(type2.HalfDamageFrom, 0.5);
            UpdateEffectivenessFrom(type2.NoDamageFrom, 0.0);
        }

        UpdateEffectivenessTo(type1.DoubleDamageTo, 2.0);
        UpdateEffectivenessTo(type1.HalfDamageTo, 0.5);
        UpdateEffectivenessTo(type1.NoDamageTo, 0.0);

        if (type2 != null)
        {
            UpdateEffectivenessTo(type2.DoubleDamageTo, 2.0);
            UpdateEffectivenessTo(type2.HalfDamageTo, 0.5);
            UpdateEffectivenessTo(type2.NoDamageTo, 0.0);
        }

        // Create the final effectiveness object
        var finalEffectiveness = new PokemonEffectiveness();

        foreach (var kvp in effectivenessFromDict)
        {
            if (kvp.Value == 2.0)
            {
                finalEffectiveness.DoubleDamageFrom.Add(kvp.Key);
            }
            else if (kvp.Value == 0.5)
            {
                finalEffectiveness.HalfDamageFrom.Add(kvp.Key);
            }
            else if (kvp.Value == 0.0)
            {
                finalEffectiveness.NoDamageFrom.Add(kvp.Key);
            }
        }

        foreach (var kvp in effectivenessToDict)
        {
            if (kvp.Value == 2.0)
            {
                finalEffectiveness.DoubleDamageTo.Add(kvp.Key);
            }
            else if (kvp.Value == 0.5)
            {
                finalEffectiveness.HalfDamageTo.Add(kvp.Key);
            }
            else if (kvp.Value == 0.0)
            {
                finalEffectiveness.NoDamageTo.Add(kvp.Key);
            }

        }

        return finalEffectiveness;
    }

    public static PokemonEffectiveness CalculateCombinedEffectiveness(PokemonType primaryType, PokemonType? secondaryType = null)
    {
        var combinedDoubleDamageFrom = new HashSet<string>();
        var combinedHalfDamageFrom = new HashSet<string>();
        var combinedNoDamageFrom = new HashSet<string>();

        var combinedDoubleDamageTo = new HashSet<string>();
        var combinedHalfDamageTo = new HashSet<string>();
        var combinedNoDamageTo = new HashSet<string>();

        combinedDoubleDamageFrom.UnionWith(primaryType.DoubleDamageFrom);
        combinedHalfDamageFrom.UnionWith(primaryType.HalfDamageFrom);
        combinedNoDamageFrom.UnionWith(primaryType.NoDamageFrom);

        combinedDoubleDamageTo.UnionWith(primaryType.DoubleDamageTo);
        combinedHalfDamageTo.UnionWith(primaryType.HalfDamageTo);
        combinedNoDamageTo.UnionWith(primaryType.NoDamageTo);

        if (secondaryType != null)
        {
            combinedDoubleDamageFrom.UnionWith(secondaryType.DoubleDamageFrom);
            combinedHalfDamageFrom.UnionWith(secondaryType.HalfDamageFrom);
            combinedNoDamageFrom.UnionWith(secondaryType.NoDamageFrom);

            combinedDoubleDamageTo.UnionWith(secondaryType.DoubleDamageTo);
            combinedHalfDamageTo.UnionWith(secondaryType.HalfDamageTo);
            combinedNoDamageTo.UnionWith(secondaryType.NoDamageTo);
        }


        // Calculate final effectiveness
        var finalDoubleDamageFrom = new HashSet<string>(combinedDoubleDamageFrom);
        finalDoubleDamageFrom.ExceptWith(combinedNoDamageFrom);
        finalDoubleDamageFrom.ExceptWith(combinedHalfDamageFrom);

        var finalHalfDamageFrom = new HashSet<string>(combinedHalfDamageFrom);
        finalHalfDamageFrom.ExceptWith(combinedNoDamageFrom);
        finalHalfDamageFrom.ExceptWith(combinedDoubleDamageFrom);

        var finalDoubleDamageTo = new HashSet<string>(combinedDoubleDamageTo);
        finalDoubleDamageTo.ExceptWith(combinedNoDamageTo);
        finalDoubleDamageTo.ExceptWith(combinedHalfDamageTo);

        var finalHalfDamageTo = new HashSet<string>(combinedHalfDamageTo);
        finalHalfDamageTo.ExceptWith(combinedNoDamageTo);
        finalHalfDamageTo.ExceptWith(combinedDoubleDamageTo);

        Console.WriteLine("DoubleDamageFrom: " + string.Join(", ", finalDoubleDamageFrom));
        Console.WriteLine("HalfDamageFrom: " + string.Join(", ", finalHalfDamageFrom));
        Console.WriteLine("NoDamageFrom: " + string.Join(", ", combinedNoDamageFrom));

        Console.WriteLine("DoubleDamageTo: " + string.Join(", ", finalDoubleDamageTo));
        Console.WriteLine("HalfDamageTo: " + string.Join(", ", finalHalfDamageTo));
        Console.WriteLine("NoDamageTo: " + string.Join(", ", combinedNoDamageTo));
        return new(
            finalDoubleDamageFrom.ToList(),
            finalHalfDamageFrom.ToList(),
            combinedNoDamageFrom.ToList(),
            finalDoubleDamageTo.ToList(),
            finalHalfDamageTo.ToList(),
            combinedNoDamageTo.ToList()
        );
    }

    public static async Task<List<string>> GetAllTypes()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{POKEMON_TYPE_API_URL}");
            var json = JObject.Parse(response);
            var types = json["results"].Select(t => t["name"].ToString()).ToList();

            return types;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new List<string>();
        }
    }

    /// <summary>
    /// Fetches move details from PokeAPI with caching
    /// </summary>
    public static async Task<PokemonMove?> GetMoveDetails(string moveName)
    {
        try
        {
            // Check cache first
            string cacheKey = moveName.ToLower().Replace(" ", "-");
            if (_moveCache.ContainsKey(cacheKey))
            {
                return _moveCache[cacheKey];
            }

            // Fetch from API
            var response = await _httpClient.GetStringAsync($"https://pokeapi.co/api/v2/move/{cacheKey}");
            var json = JObject.Parse(response);

            var move = new PokemonMove
            {
                Name = char.ToUpper(moveName[0]) + moveName.Substring(1).Replace("-", " "),
                Power = (int)(json["power"] ?? 0),
                PP = (int)(json["pp"] ?? 0),
                Accuracy = (int)(json["accuracy"] ?? 0),
                Description = json["effect_entries"]?[0]?["short_effect"]?.ToString() ?? "No description available"
            };

            // Cache it
            _moveCache[cacheKey] = move;
            return move;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to fetch move {moveName}: {e.Message}");
            return null;
        }
    }

}
