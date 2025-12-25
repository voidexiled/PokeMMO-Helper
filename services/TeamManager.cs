using Newtonsoft.Json;
using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// Manages saving and loading of Pokemon teams
/// </summary>
public static class TeamManager
{
    private static readonly string TeamsDirectory = "teams";

    /// <summary>
    /// Ensures the teams directory exists
    /// </summary>
    private static void EnsureTeamsDirectory()
    {
        if (!Directory.Exists(TeamsDirectory))
        {
            Directory.CreateDirectory(TeamsDirectory);
        }
    }

    /// <summary>
    /// Saves a team to a JSON file
    /// </summary>
    public static void SaveTeam(List<PlayerPokemon> team, string filename)
    {
        try
        {
            EnsureTeamsDirectory();

            // Sanitize filename
            filename = filename.Trim();
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("Filename cannot be empty");
            }

            // Add .json extension if not present
            if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".json";
            }

            var filePath = Path.Combine(TeamsDirectory, filename);
            var json = JsonConvert.SerializeObject(team, Formatting.Indented);
            File.WriteAllText(filePath, json);

            Console.WriteLine($"Team saved to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save team: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads a team from a JSON file
    /// </summary>
    public static List<PlayerPokemon>? LoadTeam(string filename)
    {
        try
        {
            // Add .json extension if not present
            if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".json";
            }

            var filePath = Path.Combine(TeamsDirectory, filename);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Team file not found: {filePath}");
                return null;
            }

            var json = File.ReadAllText(filePath);
            var team = JsonConvert.DeserializeObject<List<PlayerPokemon>>(json);

            Console.WriteLine($"Team loaded from {filePath}");
            return team;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load team: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets a list of all saved team files
    /// </summary>
    public static List<string> GetSavedTeams()
    {
        try
        {
            EnsureTeamsDirectory();

            var files = Directory.GetFiles(TeamsDirectory, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get saved teams: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Deletes a saved team
    /// </summary>
    public static bool DeleteTeam(string filename)
    {
        try
        {
            // Add .json extension if not present
            if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".json";
            }

            var filePath = Path.Combine(TeamsDirectory, filename);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Team deleted: {filePath}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete team: {ex.Message}");
            return false;
        }
    }
}
