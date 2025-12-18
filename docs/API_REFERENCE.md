# API Reference

Referencia completa de todos los servicios públicos del backend.

---

## Table of Contents

- [StatsCalculator](#statscalculator)
- [TeamService](#teamservice)
- [ConfigService](#configservice)
- [Nature](#nature)
- [Ability](#ability)
- [Item](#item)

---

## StatsCalculator

Servicio para calcular stats de Pokémon usando las fórmulas oficiales.

### Methods

#### `CalculateFinalStats()`
Calcula todos los stats finales de un Pokémon.

```csharp
public static PokemonStats CalculateFinalStats(
    PokemonStats baseStats,
    PokemonStats ivs,
    PokemonStats evs,
    Nature nature,
    int level)
```

**Parameters:**
- `baseStats`: Stats base de la especie
- `ivs`: Individual Values (0-31)
- `evs`: Effort Values (0-252, total max 510)
- `nature`: Naturaleza del Pokémon
- `level`: Nivel (1-100)

**Returns:** `PokemonStats` con stats finales calculados

**Example:**
```csharp
var finalStats = StatsCalculator.CalculateFinalStats(
    charizard.BaseStats,
    new PokemonStats(31, 0, 31, 31, 31, 31), // IVs
    new PokemonStats(4, 0, 0, 252, 0, 252),  // EVs
    Nature.GetNature("Timid"),
    50
);
Console.WriteLine($"HP: {finalStats.HP}"); // Output: HP: 153
```

#### `ValidateEVs()`
Valida que los EVs estén dentro de límites permitidos.

```csharp
public static (bool isValid, string error) ValidateEVs(PokemonStats evs)
```

**Returns:** Tupla con bool de validez y mensaje de error si aplica

**Rules:**
- Cada stat: 0-252
- Total: máximo 510

**Example:**
```csharp
var evs = new PokemonStats(252, 0, 0, 252, 0, 0); // 504 total
var (isValid, error) = StatsCalculator.ValidateEVs(evs);
if (isValid)
    Console.WriteLine("EVs are valid!");
```

#### `ValidateIVs()`
Valida que los IVs estén entre 0-31.

```csharp
public static (bool isValid, string error) ValidateIVs(PokemonStats ivs)
```

#### `GetTotalEVs()`
Obtiene el total de EVs invertidos.

```csharp
public static int GetTotalEVs(PokemonStats evs)
```

**Example:**
```csharp
int total = StatsCalculator.GetTotalEVs(myPokemon.EVs);
Console.WriteLine($"Total EVs: {total}/510");
```

---

## TeamService

Servicio para gestionar el equipo del jugador (máximo 6 Pokémon).

### Properties

#### `CurrentTeam`
Obtiene una copia read-only del equipo actual.

```csharp
public static List<PlayerPokemon> CurrentTeam { get; }
```

#### `TeamSize`
Número de Pokémon en el equipo (0-6).

```csharp
public static int TeamSize { get; }
```

### Methods

#### `AddPokemon()`
Agrega un Pokémon al equipo.

```csharp
public static (bool success, string error) AddPokemon(PlayerPokemon pokemon)
```

**Validations:**
- Equipo no debe estar lleno (max 6)
- IVs deben ser válidos (0-31)
- EVs deben ser válidos (0-252, total 510)
- Moveset máximo 4 movimientos

**Example:**
```csharp
var (success, error) = TeamService.AddPokemon(myCharizard);
if (!success)
    Console.WriteLine($"Error: {error}");
```

#### `RemovePokemon()`
Remueve un Pokémon por posición.

```csharp
public static bool RemovePokemon(int position)
```

**Parameters:**
- `position`: Posición en el equipo (1-6)

#### `UpdatePokemon()`
Actualiza un Pokémon existente.

```csharp
public static bool UpdatePokemon(int position, PlayerPokemon pokemon)
```

#### `GetPokemon()`
Obtiene un Pokémon por posición.

```csharp
public static PlayerPokemon? GetPokemon(int position)
```

**Returns:** Pokemon en la posición o `null` si está vacío

**Example:**
```csharp
var firstPokemon = TeamService.GetPokemon(1);
if (first Pokemon != null)
    Console.WriteLine($"Lead: {firstPokemon.GetDisplayName()}");
```

#### `ClearTeam()`
Limpia todo el equipo.

```csharp
public static void ClearTeam()
```

#### `ValidateTeam()`
Valida que el equipo sea válido para batalla.

```csharp
public static (bool isValid, List<string> errors) ValidateTeam()
```

**Checks:**
- Equipo no está vacío
- Todos los Pokémon tienen al menos 1 movimiento
- Ningún Pokémon está debilitado

#### `GetActivePokemon()`
Obtiene Pokémon que no están debilitados.

```csharp
public static List<PlayerPokemon> GetActivePokemon()
```

#### `SwapPokemon()`
Intercambia dos Pokémon de posición.

```csharp
public static bool SwapPokemon(int position1, int position2)
```

---

## ConfigService

Servicio para gestionar configuración y persistencia.

### Properties

#### `Settings`
Configuración actual de la aplicación.

```csharp
public static AppSettings Settings { get; set; }
```

### Methods

#### `Initialize()`
Inicializa el servicio (llamar al inicio de la app).

```csharp
public static void Initialize()
```

**Effects:**
- Crea directorios `data/teams` y `data/config`
- Carga settings si existen
- Crea settings por defecto si no existen

#### `SaveTeam()`
Guarda el equipo actual en JSON.

```csharp
public static bool SaveTeam(string teamName)
```

**File Location:** `data/teams/{teamName}.json`

**Example:**
```csharp
if (ConfigService.SaveTeam("Main Team"))
    Console.WriteLine("Team saved!");
```

#### `LoadTeam()`
Carga un equipo desde JSON.

```csharp
public static bool LoadTeam(string teamName)
```

**Example:**
```csharp
if (ConfigService.LoadTeam("Main Team"))
{
    Console.WriteLine($"Loaded {TeamService.TeamSize} Pokemon");
}
```

#### `GetSavedTeams()`
Lista todos los equipos guardados.

```csharp
public static List<string> GetSavedTeams()
```

**Example:**
```csharp
var teams = ConfigService.GetSavedTeams();
foreach (var team in teams)
{
    Console.WriteLine($"- {team}");
}
```

#### `DeleteTeam()`
Elimina un equipo guardado.

```csharp
public static bool DeleteTeam(string teamName)
```

#### `SaveSettings()`
Guarda la configuración de la app.

```csharp
public static bool SaveSettings()
```

**File Location:** `data/config/app_settings.json`

**Example:**
```csharp
ConfigService.Settings.Battle.EnableBattleAssistant = true;
ConfigService.SaveSettings();
```

#### `LoadSettings()`
Carga la configuración de la app.

```csharp
public static bool LoadSettings()
```

---

## Nature

Clase para naturalezas de Pokémon.

### Static Methods

#### `GetAllNatures()`
Obtiene las 25 naturalezas.

```csharp
public static List<Nature> GetAllNatures()
```

**Example:**
```csharp
var natures = Nature.GetAllNatures();
foreach (var nature in natures)
{
    Console.WriteLine(nature.ToString());
}
// Output: Timid (+speed, -attack), Adamant (+attack, -specialattack), ...
```

#### `GetNature()`
Obtiene una naturaleza por nombre.

```csharp
public static Nature? GetNature(string name)
```

**Example:**
```csharp
var timid = Nature.GetNature("Timid");
double speedMult = timid.GetMultiplier("speed"); // 1.1
double attackMult = timid.GetMultiplier("attack"); // 0.9
```

### Instance Methods

#### `GetMultiplier()`
Obtiene el multiplicador para un stat (0.9, 1.0, o 1.1).

```csharp
public double GetMultiplier(string statName)
```

#### `IsNeutral()`
Retorna true si la naturaleza es neutral.

```csharp
public bool IsNeutral()
```

---

## Ability

Clase para habilidades de Pokémon.

### Static Methods

#### `GetCommonAbilities()`
Obtiene lista de habilidades comunes.

```csharp
public static List<Ability> GetCommonAbilities()
```

**Example:**
```csharp
var abilities = Ability.GetCommonAbilities();
// Blaze, Overgrow, Torrent, Intimidate, Levitate, etc.
```

---

## Item

Clase para ítems equipables.

### Static Methods

#### `GetCommonItems()`
Obtiene lista de ítems comunes.

```csharp
public static List<Item> GetCommonItems()
```

**Example:**
```csharp
var items = Item.GetCommonItems();
// Choice Band, Life Orb, Leftovers, Focus Sash, etc.
```

#### `GetItem()`
Obtiene un ítem por nombre.

```csharp
public static Item? GetItem(string name)
```

---

## Enums

### PokemonGender
```csharp
public enum PokemonGender
{
    Male,
    Female,
    Genderless
}
```

### MoveDamageClass
```csharp
public enum MoveDamageClass
{
    Physical,  // Usa Attack y Defense
    Special,   // Usa Special Attack y Special Defense
    Status     // No causa daño
}
```

### MoveTarget
```csharp
public enum MoveTarget
{
    SelectedPokemon,
    AllOpponents,
    User,
    // ... etc
}
```

---

## Data Models

### PokemonStats
```csharp
public class PokemonStats
{
    public int HP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int SpecialAttack { get; set; }
    public int SpecialDefense { get; set; }
    public int Speed { get; set; }
    
    // Helper methods
    public int GetStat(string statName)
    public void SetStat(string statName, int value)
    public PokemonStats Clone()
}
```

### PlayerPokemon
```csharp
public class PlayerPokemon
{
    // Basic
    public string Nickname { get; set; }
    public Pokemon BaseData { get; set; }
    public int Level { get; set; }
    public PokemonGender Gender { get; set; }
    
    // Battle
    public Nature Nature { get; set; }
    public Ability Ability { get; set; }
    public Item? HeldItem { get; set; }
    
    // Stats
    public PokemonStats BaseStats { get; set; }
    public PokemonStats IVs { get; set; }
    public PokemonStats EVs { get; set; }
    public PokemonStats CalculatedStats { get; set; }
    
    // Moveset
    public List<LearnedMove> Moveset { get; set; }
    
    // Team
    public int Order { get; set; } // 1-6
    
    // Helper methods
    public bool AddMove(LearnedMove move)
    public string GetDisplayName()
    public double GetHPPercentage()
    public bool IsFainted()
}
```

---

## Thread Safety

Todos los servicios son thread-safe:
- `TeamService` usa locks internos
- `ConfigService` usa locks para Settings
- Puedes llamar desde cualquier thread sin preocuparte

---

## Performance Notes

- `CalculateFinalStats()`: ~0.1ms (safe para llamar cada frame)
- `SaveTeam()/LoadTeam()`: ~10-50ms (I/O bound)
- `PokemonService.GetPokemon()`: ~200-500ms (network call)

---

## Error Handling

La mayoría de métodos retornan tuplas `(bool success, string error)`:

```csharp
var (success, error) = TeamService.AddPokemon(pokemon);
if (!success)
{
    // Mostrar error al usuario
    ImGui.TextColored(Color.Red, error);
}
```

Métodos como `LoadTeam()` retornan `bool` simple. Revisa la consola para errores detallados.
