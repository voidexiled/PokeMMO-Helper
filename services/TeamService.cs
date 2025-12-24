using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// Service for managing the player's Pokemon team
/// Handles CRUD operations and validation for the team (max 6 Pokemon)
/// </summary>
public static class TeamService
{
    private static List<PlayerPokemon> _currentTeam = new();
    private static readonly object _teamLock = new();
    private static string _currentTeamName = "Unnamed Team";

    /// <summary>
    /// Gets or sets the current team's name
    /// </summary>
    public static string CurrentTeamName
    {
        get
        {
            lock (_teamLock)
            {
                return _currentTeamName;
            }
        }
        set
        {
            lock (_teamLock)
            {
                _currentTeamName = value ?? "Unnamed Team";
            }
        }
    }

    /// <summary>
    /// Gets the current team (read-only copy)
    /// </summary>
    public static List<PlayerPokemon> CurrentTeam
    {
        get
        {
            lock (_teamLock)
            {
                return new List<PlayerPokemon>(_currentTeam);
            }
        }
    }

    /// <summary>
    /// Gets the number of Pokemon in the team
    /// </summary>
    public static int TeamSize
    {
        get
        {
            lock (_teamLock)
            {
                return _currentTeam.Count;
            }
        }
    }

    /// <summary>
    /// Adds a Pokemon to the team (maximum 6)
    /// </summary>
    /// <returns>Tuple of (success, error message)</returns>
    public static (bool success, string error) AddPokemon(PlayerPokemon pokemon)
    {
        lock (_teamLock)
        {
            if (_currentTeam.Count >= 6)
                return (false, "Team is full (maximum 6 Pokemon)");

            if (pokemon == null)
                return (false, "Pokemon cannot be null");

            if (pokemon.BaseData == null)
                return (false, "Pokemon must have base data");

            // Validate stats
            var ivValidation = StatsCalculator.ValidateIVs(pokemon.IVs);
            if (!ivValidation.isValid)
                return (false, $"Invalid IVs: {ivValidation.error}");

            var evValidation = StatsCalculator.ValidateEVs(pokemon.EVs);
            if (!evValidation.isValid)
                return (false, $"Invalid EVs: {evValidation.error}");

            // Validate moveset
            if (pokemon.Moveset.Count > 4)
                return (false, "Pokemon cannot have more than 4 moves");

            // Set order
            pokemon.Order = _currentTeam.Count + 1;

            // Calculate final stats
            pokemon.CalculatedStats = StatsCalculator.CalculateFinalStats(
                pokemon.BaseStats,
                pokemon.IVs,
                pokemon.EVs,
                pokemon.Nature,
                pokemon.Level
            );

            // Set current HP to max HP if not set
            if (pokemon.CurrentHP == 0)
                pokemon.CurrentHP = pokemon.CalculatedStats.HP;

            _currentTeam.Add(pokemon);
            return (true, "");
        }
    }

    /// <summary>
    /// Removes a Pokemon from the team by position (1-6)
    /// </summary>
    public static bool RemovePokemon(int position)
    {
        lock (_teamLock)
        {
            if (position < 1 || position > _currentTeam.Count)
                return false;

            _currentTeam.RemoveAt(position - 1);

            // Update orders
            for (int i = 0; i < _currentTeam.Count; i++)
            {
                _currentTeam[i].Order = i + 1;
            }

            return true;
        }
    }

    /// <summary>
    /// Updates a Pokemon in the team
    /// </summary>
    public static bool UpdatePokemon(int position, PlayerPokemon pokemon)
    {
        lock (_teamLock)
        {
            if (position < 1 || position > _currentTeam.Count)
                return false;

            if (pokemon == null)
                return false;

            // Recalculate stats
            pokemon.CalculatedStats = StatsCalculator.CalculateFinalStats(
                pokemon.BaseStats,
                pokemon.IVs,
                pokemon.EVs,
                pokemon.Nature,
                pokemon.Level
            );

            pokemon.Order = position;
            _currentTeam[position - 1] = pokemon;
            return true;
        }
    }

    /// <summary>
    /// Gets a Pokemon by position (1-6)
    /// </summary>
    public static PlayerPokemon? GetPokemon(int position)
    {
        lock (_teamLock)
        {
            if (position < 1 || position > _currentTeam.Count)
                return null;

            return _currentTeam[position - 1];
        }
    }

    /// <summary>
    /// Clears the entire team
    /// </summary>
    public static void ClearTeam()
    {
        lock (_teamLock)
        {
            _currentTeam.Clear();
            _currentTeamName = "Unnamed Team";
        }
    }

    /// <summary>
    /// Validates that the team is valid for battle
    /// </summary>
    public static (bool isValid, List<string> errors) ValidateTeam()
    {
        lock (_teamLock)
        {
            var errors = new List<string>();

            if (_currentTeam.Count == 0)
                errors.Add("Team is empty");

            foreach (var pokemon in _currentTeam)
            {
                if (pokemon.Moveset.Count == 0)
                    errors.Add($"{pokemon.GetDisplayName()} has no moves");

                if (pokemon.IsFainted())
                    errors.Add($"{pokemon.GetDisplayName()} has fainted");
            }

            return (errors.Count == 0, errors);
        }
    }

    /// <summary>
    /// Gets all Pokemon that are not fainted
    /// </summary>
    public static List<PlayerPokemon> GetActivePokemon()
    {
        lock (_teamLock)
        {
            return _currentTeam.Where(p => !p.IsFainted()).ToList();
        }
    }

    /// <summary>
    /// Swaps two Pokemon positions
    /// </summary>
    public static bool SwapPokemon(int position1, int position2)
    {
        lock (_teamLock)
        {
            if (position1 < 1 || position1 > _currentTeam.Count)
                return false;
            if (position2 < 1 || position2 > _currentTeam.Count)
                return false;

            var temp = _currentTeam[position1 - 1];
            _currentTeam[position1 - 1] = _currentTeam[position2 - 1];
            _currentTeam[position2 - 1] = temp;

            // Update orders
            _currentTeam[position1 - 1].Order = position1;
            _currentTeam[position2 - 1].Order = position2;

            return true;
        }
    }

    /// <summary>
    /// Sets the entire team (used for loading from file)
    /// </summary>
    internal static void SetTeam(List<PlayerPokemon> team)
    {
        lock (_teamLock)
        {
            _currentTeam = team ?? new List<PlayerPokemon>();

            // Ensure orders are correct
            for (int i = 0; i < _currentTeam.Count; i++)
            {
                _currentTeam[i].Order = i + 1;
            }
        }
    }
}
