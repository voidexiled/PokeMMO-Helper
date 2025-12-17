namespace PasaporteFiller.core;

/// <summary>
/// Common enums used throughout the application
/// </summary>

/// <summary>
/// Pokemon gender
/// </summary>
public enum PokemonGender
{
    Male,
    Female,
    Genderless
}

/// <summary>
/// Move damage class (Physical, Special, or Status)
/// </summary>
public enum MoveDamageClass
{
    Physical,  // Uses Attack and Defense stats
    Special,   // Uses Special Attack and Special Defense stats
    Status     // No damage, causes effects
}

/// <summary>
/// Move target (who the move affects)
/// </summary>
public enum MoveTarget
{
    SelectedPokemon,      // Single target
    AllOpponents,         // All opposing Pokemon
    AllOtherPokemon,      // All Pokemon except user
    User,                 // Self
    UserAndAllies,        // User and allies
    AllPokemon,           // All Pokemon on field
    RandomOpponent,       // Random opponent
    AllAllies,            // All allies
    UserOrAlly,           // User or selected ally
    OpponentsField,       // Affects opposing side (entry hazards, etc.)
    UsersField,           // Affects user's side
    EntireField           // Affects whole battlefield (weather, terrain)
}
