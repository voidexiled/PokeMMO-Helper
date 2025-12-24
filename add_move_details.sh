#!/bin/bash
# Add move cache and GetMoveDetails method to PokemonService.cs

cd "$(dirname "$0")"

# Add move cache dictionary after _httpClient (line 20)
sed -i '20 a\    private static readonly Dictionary<string, PokemonMove> _moveCache = new Dictionary<string, PokemonMove>();' services/PokemonService.cs

# Add GetMoveDetails method before the closing brace (line 360)
sed -i '359 a\
\
    /// <summary>\
    /// Fetches move details from PokeAPI with caching\
    /// </summary>\
    public static async Task<PokemonMove?> GetMoveDetails(string moveName)\
    {\
        try\
        {\
            // Check cache first\
            string cacheKey = moveName.ToLower().Replace(" ", "-");\
            if (_moveCache.ContainsKey(cacheKey))\
            {\
                return _moveCache[cacheKey];\
            }\
\
            // Fetch from API\
            var response = await _httpClient.GetStringAsync($"https://pokeapi.co/api/v2/move/{cacheKey}");\
            var json = JObject.Parse(response);\
\
            var move = new PokemonMove\
            {\
                Name = char.ToUpper(moveName[0]) + moveName.Substring(1).Replace("-", " "),\
                Power = (int)(json["power"] ?? 0),\
                PP = (int)(json["pp"] ?? 0),\
                Accuracy = (int)(json["accuracy"] ?? 0),\
                Description = json["effect_entries"]?[0]?["short_effect"]?.ToString() ?? "No description available"\
            };\
\
            // Cache it\
            _moveCache[cacheKey] = move;\
            return move;\
        }\
        catch (Exception e)\
        {\
            Console.WriteLine($"Failed to fetch move {moveName}: {e.Message}");\
            return null;\
        }\
    }' services/PokemonService.cs

echo "✅ GetMoveDetails added!"
