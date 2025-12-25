using Newtonsoft.Json;
using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// Cache data structure for disk storage
/// </summary>
public class CacheData
{
    public List<Pokemon> AllPokemon { get; set; } = new();
    public List<Item> AllItems { get; set; } = new();
    public List<Ability> AllAbilities { get; set; } = new();
    public Dictionary<string, Pokemon> PokemonCache { get; set; } = new();  // NEW: Full Pokemon with moves
    public DateTime CacheDate { get; set; }
    public string Version { get; set; } = "2.0";  // Bumped for new structure
}

/// <summary>
/// Manages disk caching of API data for fast subsequent loads
/// </summary>
public static class CacheManager
{
    private static readonly string _dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");
    private static readonly string _cacheFile = Path.Combine(_dataDirectory, "api_cache.json");
    private const int CACHE_VALID_DAYS = 7;

    /// <summary>
    /// Saves all API data to disk
    /// </summary>
    public static bool SaveCache(CacheData data)
    {
        try
        {
            // Ensure directory exists
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            data.CacheDate = DateTime.Now;
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_cacheFile, json);

            Console.WriteLine($"Cache saved to {_cacheFile}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save cache: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads API data from disk cache
    /// </summary>
    public static CacheData? LoadCache()
    {
        try
        {
            if (!File.Exists(_cacheFile))
            {
                Console.WriteLine("No cache file found");
                return null;
            }

            string json = File.ReadAllText(_cacheFile);
            var cache = JsonConvert.DeserializeObject<CacheData>(json);

            if (cache == null)
            {
                Console.WriteLine("Failed to deserialize cache");
                return null;
            }

            Console.WriteLine($"Cache loaded from {_cacheFile}");
            return cache;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load cache: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if cache is still valid (not expired)
    /// </summary>
    public static bool IsCacheValid(CacheData cache)
    {
        if (cache == null) return false;

        var age = DateTime.Now - cache.CacheDate;
        bool isValid = age.TotalDays < CACHE_VALID_DAYS;

        if (!isValid)
        {
            Console.WriteLine($"Cache expired (age: {age.TotalDays:F1} days)");
        }

        return isValid;
    }

    /// <summary>
    /// Deletes the cache file
    /// </summary>
    public static bool ClearCache()
    {
        try
        {
            if (File.Exists(_cacheFile))
            {
                File.Delete(_cacheFile);
                Console.WriteLine("Cache cleared");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear cache: {ex.Message}");
            return false;
        }
    }
}
