namespace PasaporteFiller.core;

/// <summary>
/// Utility class for getting Pokemon type icon URLs
/// </summary>
public static class TypeIcons
{
    // PokeAPI sprites repository for type icons
    private const string BaseUrl = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/types/generation-viii/brilliant-diamond-and-shining-pearl/";

    /// <summary>
    /// Get the icon URL for a Pokemon type
    /// </summary>
    /// <param name="typeName">Type name (e.g., "fire", "water")</param>
    /// <returns>URL to type icon PNG</returns>
    public static string GetTypeIconUrl(string typeName)
    {
        var typeId = GetTypeId(typeName.ToLower());
        return typeId > 0 ? $"{BaseUrl}{typeId}.png" : "";
    }

    /// <summary>
    /// Convert type name to PokeAPI type ID
    /// </summary>
    private static int GetTypeId(string typeName)
    {
        return typeName switch
        {
            "normal" => 1,
            "fighting" => 2,
            "flying" => 3,
            "poison" => 4,
            "ground" => 5,
            "rock" => 6,
            "bug" => 7,
            "ghost" => 8,
            "steel" => 9,
            "fire" => 10,
            "water" => 11,
            "grass" => 12,
            "electric" => 13,
            "psychic" => 14,
            "ice" => 15,
            "dragon" => 16,
            "dark" => 17,
            "fairy" => 18,
            _ => 0 // Unknown type
        };
    }

    /// <summary>
    /// Check if a type name is valid
    /// </summary>
    public static bool IsValidType(string typeName)
    {
        return GetTypeId(typeName.ToLower()) > 0;
    }
}
