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
    private static string POKEMON_ITEM_API_URL = "https://pokeapi.co/api/v2/item/";
    private static string POKEMON_ABILITY_API_URL = "https://pokeapi.co/api/v2/ability/";

    private static string POKEMON_IMAGE_URL = "https://raw.githubusercontent.com/duiker101/pokemon-type-svg-icons/master/icons/";

    private static readonly Dictionary<string, PokemonMove> _moveCache = new Dictionary<string, PokemonMove>();
    private static readonly Dictionary<string, Item> _itemCache = new Dictionary<string, Item>();
    private static readonly Dictionary<string, Ability> _abilityCache = new Dictionary<string, Ability>();
    private static readonly Dictionary<string, Pokemon> _pokemonCache = new Dictionary<string, Pokemon>();

    private static List<Item>? _allItems = null;
    private static List<string>? _allPokemon = null;
    private static List<Ability>? _allAbilities = null;

    public static async Task<List<String>> GetPokemonList()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{POKEMON_API_URL}?limit=1500");
            var json = JObject.Parse(response);
            var pokemonList = json["results"].Select(p => p["name"].ToString()).Where(name => !name.Contains("-")).ToList();

            // CRITICAL: Assign to static field!
            _allPokemon = pokemonList;

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
        var cacheKey = name.ToLower();

        // Check cache first - instant return if found!
        if (_pokemonCache.ContainsKey(cacheKey))
        {
            return _pokemonCache[cacheKey];
        }

        // Not in cache - load from API
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

            // Extract base stats from Pok├®API
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

            // Extract sprite URLs
            var sprites = json["sprites"];
            var spriteUrl = sprites?["front_default"]?.ToString();
            var spriteShinyUrl = sprites?["front_shiny"]?.ToString();

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
                baseStats,  // Ô¡É NEW: Pass base stats to constructor
                CombinedEffectiveness.DoubleDamageFrom,
                CombinedEffectiveness.HalfDamageFrom,
                CombinedEffectiveness.NoDamageFrom,
                CombinedEffectiveness.DoubleDamageTo,
                CombinedEffectiveness.HalfDamageTo,
                CombinedEffectiveness.NoDamageTo,
                allTypes

            );

            // Assign sprite URLs
            pokemon.SpriteUrl = spriteUrl;
            pokemon.SpriteShinyUrl = spriteShinyUrl;

            // Cache it for next time!
            _pokemonCache[cacheKey] = pokemon;

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
            return null;
        }
    }

    #region Items

    public static async Task<List<Item>> GetAllItems()
    {
        if (_allItems != null) return _allItems;

        try
        {
            var countResponse = await _httpClient.GetStringAsync($"{POKEMON_ITEM_API_URL}?limit=1");
            var countJson = JObject.Parse(countResponse);
            int totalCount = countJson["count"]?.Value<int>() ?? 2000;

            var response = await _httpClient.GetStringAsync($"{POKEMON_ITEM_API_URL}?limit={totalCount}");
            var json = JObject.Parse(response);
            var items = new List<Item>();

            items.Add(new Item("None", ItemCategory.None, "No item equipped", ""));

            foreach (var itemObj in json["results"])
            {
                string itemName = itemObj["name"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(itemName))
                {
                    items.Add(new Item(FormatItemName(itemName), ItemCategory.Other, "", ""));
                }
            }

            _allItems = items;
            return items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch items: {ex.Message}");
            return Item.GetCommonItems();
        }
    }

    public static List<Item> GetCachedItems()
    {
        return _allItems ?? new List<Item>();
    }

    public static async Task<Item?> GetItemDetails(string itemName)
    {
        if (_itemCache.ContainsKey(itemName)) return _itemCache[itemName];

        try
        {
            var response = await _httpClient.GetStringAsync($"{POKEMON_ITEM_API_URL}{itemName.ToLower().Replace(" ", "-")}");
            var json = JObject.Parse(response);

            string category = json["category"]?["name"]?.ToString() ?? "other";
            string effect = json["effect_entries"]?.FirstOrDefault()?["effect"]?.ToString() ?? "";
            string sprite = json["sprites"]?["default"]?.ToString() ?? "";

            var item = new Item(FormatItemName(itemName), MapItemCategory(category), effect, sprite);
            _itemCache[itemName] = item;
            return item;
        }
        catch
        {
            return null;
        }
    }

    private static ItemCategory MapItemCategory(string category)
    {
        return category.ToLower() switch
        {
            "stat-boosts" => ItemCategory.StatBoost,
            _ => ItemCategory.Other
        };
    }

    private static string FormatItemName(string name)
    {
        return string.Join(" ", name.Split('-')
            .Where(w => !string.IsNullOrEmpty(w))
            .Select(w => char.ToUpper(w[0]) + w.Substring(1)));
    }

    #endregion

    #region Abilities

    public static async Task<List<Ability>> GetAllAbilities()
    {
        if (_allAbilities != null) return _allAbilities;

        try
        {
            var countResponse = await _httpClient.GetStringAsync($"{POKEMON_ABILITY_API_URL}?limit=1");
            var countJson = JObject.Parse(countResponse);
            int totalCount = countJson["count"]?.Value<int>() ?? 300;

            var response = await _httpClient.GetStringAsync($"{POKEMON_ABILITY_API_URL}?limit={totalCount}");
            var json = JObject.Parse(response);
            var abilities = new List<Ability>();

            foreach (var abilityObj in json["results"])
            {
                string abilityName = abilityObj["name"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(abilityName))
                {
                    abilities.Add(new Ability(FormatAbilityName(abilityName), ""));
                }
            }

            _allAbilities = abilities;
            return abilities;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch abilities: {ex.Message}");
            return new List<Ability>();
        }
    }

    public static List<Ability> GetCachedAbilities()
    {
        return _allAbilities ?? new List<Ability>();
    }

    private static string FormatAbilityName(string name)
    {
        return string.Join(" ", name.Split('-').Select(w => char.ToUpper(w[0]) + w.Substring(1)));
    }

    public static async Task<Ability?> GetAbilityDetails(string abilityName)
    {
        if (_abilityCache.ContainsKey(abilityName)) return _abilityCache[abilityName];

        try
        {
            var response = await _httpClient.GetStringAsync($"{POKEMON_ABILITY_API_URL}{abilityName.ToLower().Replace(" ", "-")}");
            var json = JObject.Parse(response);

            string effect = json["effect_entries"]?.FirstOrDefault(e => e["language"]?["name"]?.ToString() == "en")?["effect"]?.ToString() ?? "";

            var ability = new Ability(FormatAbilityName(abilityName), effect);
            _abilityCache[abilityName] = ability;
            return ability;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Preloading

    public static async Task PreloadAllItemDetails()
    {
        if (_allItems == null) await GetAllItems();

        int total = _allItems.Count;
        int current = 0;

        foreach (var item in _allItems)
        {
            await GetItemDetails(item.Name);
            current++;
            if (current % 100 == 0)
            {
                Console.WriteLine($"Preloaded {current}/{total} item details");
            }
        }

        Console.WriteLine("All item details preloaded!");
    }

    public static async Task PreloadAllMoveDetails()
    {
        try
        {
            var countResponse = await _httpClient.GetStringAsync($"{POKEMON_MOVE_API_URL}?limit=1");
            var countJson = JObject.Parse(countResponse);
            int totalCount = countJson["count"]?.Value<int>() ?? 900;

            var response = await _httpClient.GetStringAsync($"{POKEMON_MOVE_API_URL}?limit={totalCount}");
            var json = JObject.Parse(response);

            var moveNames = json["results"].Select(m => m["name"]?.ToString() ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList();

            int current = 0;
            foreach (var moveName in moveNames)
            {
                await GetMoveDetails(moveName);
                current++;
                if (current % 50 == 0)
                {
                    Console.WriteLine($"Preloaded {current}/{moveNames.Count} move details");
                }
            }

            Console.WriteLine("All move details preloaded!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to preload moves: {ex.Message}");
        }
    }

    public static async Task PreloadAllAbilityDetails()
    {
        if (_allAbilities == null) await GetAllAbilities();

        int current = 0;
        int total = _allAbilities.Count;

        foreach (var ability in _allAbilities)
        {
            await GetAbilityDetails(ability.Name);
            current++;
            if (current % 50 == 0)
            {
                Console.WriteLine($"Preloaded {current}/{total} ability details");
            }
        }

        Console.WriteLine("All ability details preloaded!");
    }

    #endregion

    #region Caching

    public static void LoadFromCache(CacheData cache)
    {
        _allPokemon = cache.AllPokemon.Select(p => p.Name).ToList();
        _allItems = cache.AllItems;
        _allAbilities = cache.AllAbilities;

        // Populate item cache
        foreach (var item in cache.AllItems)
        {
            if (!string.IsNullOrEmpty(item.Name) && !_itemCache.ContainsKey(item.Name))
            {
                _itemCache[item.Name] = item;
            }
        }

        // Populate ability cache
        foreach (var ability in cache.AllAbilities)
        {
            if (!string.IsNullOrEmpty(ability.Name) && !_abilityCache.ContainsKey(ability.Name))
            {
                _abilityCache[ability.Name] = ability;
            }
        }

        // Populate Pokemon cache
        if (cache.PokemonCache != null)
        {
            foreach (var kvp in cache.PokemonCache)
            {
                _pokemonCache[kvp.Key] = kvp.Value;
            }
        }

        Console.WriteLine($"Loaded from cache: {_allPokemon?.Count ?? 0} pokemon, {_allItems?.Count ?? 0} items, {_allAbilities?.Count ?? 0} abilities, {_pokemonCache.Count} full Pokemon");
    }

    public static CacheData GetCacheData()
    {
        var pokemonObjects = _allPokemon?.Select(name => new Pokemon { Name = name }).ToList() ?? [];

        return new CacheData
        {
            AllPokemon = pokemonObjects,
            AllItems = _allItems ?? [],
            AllAbilities = _allAbilities ?? [],
            PokemonCache = _pokemonCache
        };
    }

    public static async Task PreloadAllPokemon(IProgress<LoadingProgress>? progress = null)
    {
        if (_allPokemon == null) await GetPokemonList();

        int total = _allPokemon.Count;
        int current = 0;
        var startTime = DateTime.Now;

        Console.WriteLine($"Pre-loading ALL {total} Pokemon with complete movesets...");
        Console.WriteLine("This will take 45-60 minutes on first run.");

        foreach (var name in _allPokemon)
        {
            try
            {
                var pokemon = await GetPokemon(name);
                if (pokemon != null)
                {
                    _pokemonCache[name.ToLower()] = pokemon;
                }

                current++;

                // Report progress with ETA every 5 Pokemon
                if (progress != null && current % 5 == 0)
                {
                    var elapsed = DateTime.Now - startTime;
                    var avgTimePerPokemon = elapsed.TotalSeconds / current;
                    var remaining = (total - current) * avgTimePerPokemon;

                    progress.Report(new LoadingProgress
                    {
                        Current = current,
                        Total = total,
                        Phase = "Pokemon",
                        EstimatedTimeRemaining = TimeSpan.FromSeconds(remaining)
                    });
                }

                // Console logging every 10
                if (current % 10 == 0)
                {
                    Console.WriteLine($"Loaded {current}/{total} Pokemon ({(current * 100.0 / total):F1}%)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load {name}: {ex.Message}");
            }
        }

        Console.WriteLine($"Completed! Loaded {_pokemonCache.Count}/{total} Pokemon");
    }

    #endregion

}
