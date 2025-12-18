using Newtonsoft.Json;
using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// Service for managing application configuration and team persistence
/// Handles saving/loading settings and teams to/from JSON files
/// </summary>
public static class ConfigService
{
    private static readonly string _dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data");
    private static readonly string _teamsDirectory = Path.Combine(_dataDirectory, "teams");
    private static readonly string _configDirectory = Path.Combine(_dataDirectory, "config");
    private static readonly string _settingsFile = Path.Combine(_configDirectory, "app_settings.json");

    private static AppSettings _settings = new();
    private static readonly object _settingsLock = new();

    /// <summary>
    /// Gets or sets the current application settings
    /// </summary>
    public static AppSettings Settings
    {
        get
        {
            lock (_settingsLock)
            {
                return _settings;
            }
        }
        set
        {
            lock (_settingsLock)
            {
                _settings = value;
            }
        }
    }

    /// <summary>
    /// Initializes the config service (creates directories if needed)
    /// </summary>
    public static void Initialize()
    {
        try
        {
            if (!Directory.Exists(_dataDirectory))
                Directory.CreateDirectory(_dataDirectory);

            if (!Directory.Exists(_teamsDirectory))
                Directory.CreateDirectory(_teamsDirectory);

            if (!Directory.Exists(_configDirectory))
                Directory.CreateDirectory(_configDirectory);

            // Load settings if they exist
            LoadSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing ConfigService: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves the current team to a JSON file
    /// </summary>
    public static bool SaveTeam(string teamName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamName))
                return false;

            // Sanitize filename
            teamName = SanitizeFileName(teamName);
            string filePath = Path.Combine(_teamsDirectory, $"{teamName}.json");

            var team = TeamService.CurrentTeam;
            var teamData = new
            {
                teamName,
                savedDate = DateTime.Now,
                pokemon = team
            };

            string json = JsonConvert.SerializeObject(teamData, Formatting.Indented);
            File.WriteAllText(filePath, json);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving team: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads a team from a JSON file
    /// </summary>
    public static bool LoadTeam(string teamName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamName))
                return false;

            teamName = SanitizeFileName(teamName);
            string filePath = Path.Combine(_teamsDirectory, $"{teamName}.json");

            if (!File.Exists(filePath))
                return false;

            string json = File.ReadAllText(filePath);
            var teamData = JsonConvert.DeserializeObject<dynamic>(json);

            if (teamData == null || teamData.pokemon == null)
                return false;

            var pokemon = JsonConvert.DeserializeObject<List<PlayerPokemon>>(teamData.pokemon.ToString());
            if (pokemon == null)
                return false;

            TeamService.SetTeam(pokemon);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading team: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets a list of all saved teams
    /// </summary>
    public static List<string> GetSavedTeams()
    {
        try
        {
            if (!Directory.Exists(_teamsDirectory))
                return new List<string>();

            var files = Directory.GetFiles(_teamsDirectory, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting saved teams: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Deletes a saved team
    /// </summary>
    public static bool DeleteTeam(string teamName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamName))
                return false;

            teamName = SanitizeFileName(teamName);
            string filePath = Path.Combine(_teamsDirectory, $"{teamName}.json");

            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting team: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Saves the application settings to file
    /// </summary>
    public static bool SaveSettings()
    {
        try
        {
            lock (_settingsLock)
            {
                string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_settingsFile, json);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads the application settings from file
    /// </summary>
    public static bool LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsFile))
            {
                // Create default settings file
                _settings = new AppSettings();
                SaveSettings();
                return true;
            }

            string json = File.ReadAllText(_settingsFile);
            var settings = JsonConvert.DeserializeObject<AppSettings>(json);

            if (settings != null)
            {
                lock (_settingsLock)
                {
                    _settings = settings;
                }
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Exports team to Pokémon Showdown format (future feature)
    /// </summary>
    public static string ExportToShowdown(string teamName)
    {
        // TODO: Implement Showdown export format
        return "";
    }

    /// <summary>
    /// Imports team from Pokémon Showdown format (future feature)
    /// </summary>
    public static bool ImportFromShowdown(string showdownText)
    {
        // TODO: Implement Showdown import format
        return false;
    }
}
