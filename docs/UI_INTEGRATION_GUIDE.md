# UI Integration Guide

Este documento explica cómo integrar los servicios del backend en tus ventanas de Dear ImGui.

---

## Tabla de Contenidos

1. [Inicialización](#inicialización)
2. [Team Manager Window](#team-manager-window)
3. [Pokemon Editor Dialog](#pokemon-editor-dialog)
4. [Battle Assistant Window](#battle-assistant-window)
5. [Configuration Window](#configuration-window)
6. [Ejemplos de Código](#ejemplos-de-código)

---

## Inicialización

Antes de usar cualquier servicio, inicializa ConfigService en el arranque de la aplicación:

```csharp
protected override void OnStart()
{
    ConfigService.Initialize(); // Crea directorios y carga settings
    // Opcional: Cargar último equipo usado
    var teams = ConfigService.GetSavedTeams();
    if (teams.Count > 0)
    {
        ConfigService.LoadTeam(teams[0]);
    }
}
```

---

## Team Manager Window

### Propósito
Ventana para gestionar el equipo de hasta 6 Pokémon.

### Servicios Usados
- `TeamService.CurrentTeam` - Lista actual del equipo
- `TeamService.AddPokemon()` - Agregar Pokémon
- `TeamService.RemovePokemon(position)` - Eliminar Pokémon
- `TeamService.GetPokemon(position)` - Obtener Pokémon
- `ConfigService.SaveTeam(name)` - Guardar equipo
- `ConfigService.LoadTeam(name)` - Cargar equipo
- `ConfigService.GetSavedTeams()` - Lista de equipos guardados

### Estructura UI Recomendada

```
┌─────────────────────────────────────┐
│ Team Manager                    [X] │
├─────────────────────────────────────┤
│ 1. [Charizard Lv.50]   [Edit] [Del]│
│ 2. [Blastoise Lv.50]   [Edit] [Del]│
│ 3. [Venusaur Lv.50]    [Edit] [Del]│
│ 4. [Empty Slot]        [Add]        │
│ 5. [Empty Slot]        [Add]        │
│ 6. [Empty Slot]        [Add]        │
├─────────────────────────────────────┤
│ Team Name: [________] [Save]        │
│ Load Team: <dropdown>  [Load]       │
└─────────────────────────────────────┘
```

### Código de Ejemplo

```csharp
// Variables de estado
private bool _showTeamManager = false;
private string _saveTeamName = "";
private int _selectedTeamIndex = 0;

// Método de renderizado
void RenderTeamManager()
{
    if (!_showTeamManager) return;
    
    ImGui.Begin("Team Manager", ref _showTeamManager);
    
    var team = TeamService.CurrentTeam;
    
    // Mostrar los 6 slots
    for (int i = 1; i <= 6; i++)
    {
        var pokemon = TeamService.GetPokemon(i);
        
        if (pokemon != null)
        {
            ImGui.Text($"{i}.");
            ImGui.SameLine();
            ImGui.Text($"{pokemon.GetDisplayName()} Lv.{pokemon.Level}");
            ImGui.SameLine();
            
            if (ImGui.Button($"Edit##{i}"))
            {
                // Abrir Pokemon Editor con este Pokemon
                OpenPokemonEditor(i);
            }
            ImGui.SameLine();
            
            if (ImGui.Button($"Del##{i}"))
            {
                TeamService.RemovePokemon(i);
            }
        }
        else
        {
            ImGui.Text($"{i}. [Empty Slot]");
            ImGui.SameLine();
            
            if (ImGui.Button($"Add##{i}"))
            {
                // Abrir Pokemon Editor para nuevo Pokemon
                OpenPokemonEditor(0); // 0 = nuevo
            }
        }
    }
    
    ImGui.Separator();
    
    // Guardar equipo
    ImGui.InputText("Team Name", ref _saveTeamName, 50);
    ImGui.SameLine();
    if (ImGui.Button("Save Team"))
    {
        if (!string.IsNullOrWhiteSpace(_saveTeamName))
        {
            bool success = ConfigService.SaveTeam(_saveTeamName);
            if (success)
                Console.WriteLine($"Team '{_saveTeamName}' saved!");
        }
    }
    
    // Cargar equipo
    var savedTeams = ConfigService.GetSavedTeams();
    if (savedTeams.Count > 0)
    {
        if (ImGui.BeginCombo("Load Team", savedTeams[_selectedTeamIndex]))
        {
            for (int i = 0; i < savedTeams.Count; i++)
            {
                bool isSelected = (_selectedTeamIndex == i);
                if (ImGui.Selectable(savedTeams[i], isSelected))
                    _selectedTeamIndex = i;
            }
            ImGui.EndCombo();
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Load"))
        {
            ConfigService.LoadTeam(savedTeams[_selectedTeamIndex]);
        }
    }
    
    ImGui.End();
}
```

---

## Pokemon Editor Dialog

### Propósito
Crear o editar un Pokémon del equipo con todos sus detalles.

### Servicios Usados
- `PokemonService.GetPokemon(name)` - Obtener datos base del Pokémon
- `Nature.GetAllNatures()` - Lista de naturalezas
- `Ability.GetCommonAbilities()` - Lista de habilidades
- `Item.GetCommonItems()` - Lista de ítems
- `StatsCalculator.CalculateFinalStats()` - Calcular stats finales
- `StatsCalculator.ValidateEVs()` - Validar EVs
- `StatsCalculator.ValidateIVs()` - Validar IVs
- `StatsCalculator.GetTotalEVs()` - Total de EVs usados
- `TeamService.AddPokemon()` - Agregar al equipo
- `TeamService.UpdatePokemon()` - Actualizar Pokemon existente

### Variables Importantes

```csharp
private bool _showPokemonEditor = false;
private int _editingPosition = 0; // 0 = nuevo, 1-6 = editar existente

// Datos del Pokemon being editado
private string _pokemonName = "";
private string _nickname = "";
private int _level = 50;
private int _genderIndex = 0; // 0=Male, 1=Female, 2=Genderless
private int _natureIndex = 0;
private int _abilityIndex = 0;
private int _itemIndex = 0;

// Stats
private PokemonStats _ivs = new(31, 31, 31, 31, 31, 31);
private PokemonStats _evs = new PokemonStats();
private PokemonStats _calculatedStats = new PokemonStats();

// Moveset
private List<string> _selectedMoves = new();
```

### Cálculo en Tiempo Real

```csharp
void CalculateStatsRealTime()
{
    if (_basePokemon == null) return;
    
    var natures = Nature.GetAllNatures();
    var nature = natures[_natureIndex];
    
    // Validar antes de calcular
    var evValidation = StatsCalculator.ValidateEVs(_evs);
    var ivValidation = StatsCalculator.ValidateIVs(_ivs);
    
    if (evValidation.isValid && ivValidation.isValid)
    {
        _calculatedStats = StatsCalculator.CalculateFinalStats(
            _basePokemon.BaseStats,
            _ivs,
            _evs,
            nature,
            _level
        );
    }
}
```

### UI de IVs/EVs

```csharp
// IVs Section
if (ImGui.TreeNode("IVs (0-31)"))
{
    ImGui.SliderInt("HP IV", ref _ivs.HP, 0, 31);
    ImGui.SliderInt("Attack IV", ref _ivs.Attack, 0, 31);
    ImGui.SliderInt("Defense IV", ref _ivs.Defense, 0, 31);
    ImGui.SliderInt("Sp.Atk IV", ref _ivs.SpecialAttack, 0, 31);
    ImGui.SliderInt("Sp.Def IV", ref _ivs.SpecialDefense, 0, 31);
    ImGui.SliderInt("Speed IV", ref _ivs.Speed, 0, 31);
    
    if (ImGui.Button("Max All IVs"))
    {
        _ivs = new PokemonStats(31, 31, 31, 31, 31, 31);
    }
    
    ImGui.TreePop();
}

// EVs Section
if (ImGui.TreeNode("EVs (0-252, Total 510)"))
{
    int totalEVs = StatsCalculator.GetTotalEVs(_evs);
    ImGui.Text($"Total EVs: {totalEVs}/510");
    
    ImGui.SliderInt("HP EV", ref _evs.HP, 0, 252);
    ImGui.SliderInt("Attack EV", ref _evs.Attack, 0, 252);
    ImGui.SliderInt("Defense EV", ref _evs.Defense, 0, 252);
    ImGui.SliderInt("Sp.Atk EV", ref _evs.SpecialAttack, 0, 252);
    ImGui.SliderInt("Sp.Def EV", ref _evs.SpecialDefense, 0, 252);
    ImGui.SliderInt("Speed EV", ref _evs.Speed, 0, 252);
    
    var evValidation = StatsCalculator.ValidateEVs(_evs);
    if (!evValidation.isValid)
    {
        ImGui.TextColored(new Vector4(1, 0, 0, 1), evValidation.error);
    }
    
    ImGui.TreePop();
}

// Recalcular stats cada frame
CalculateStatsRealTime();

// Mostrar stats finales
if (ImGui.TreeNode("Calculated Stats"))
{
    ImGui.Text($"HP: {_calculatedStats.HP}");
    ImGui.Text($"Attack: {_calculatedStats.Attack}");
    ImGui.Text($"Defense: {_calculatedStats.Defense}");
    ImGui.Text($"Sp.Atk: {_calculatedStats.SpecialAttack}");
    ImGui.Text($"Sp.Def: {_calculatedStats.SpecialDefense}");
    ImGui.Text($"Speed: {_calculatedStats.Speed}");
    ImGui.TreePop();
}
```

### Guardar Pokemon

```csharp
void SavePokemon()
{
    var pokemon = new PlayerPokemon
    {
        Nickname = _nickname,
        BaseData = _basePokemon,
        Level = _level,
        Gender = (PokemonGender)_genderIndex,
        Nature = Nature.GetAllNatures()[_natureIndex],
        Ability = Ability.GetCommonAbilities()[_abilityIndex],
        HeldItem = Item.GetCommonItems()[_itemIndex],
        IVs = _ivs.Clone(),
        EVs = _evs.Clone(),
        BaseStats = _basePokemon.BaseStats.Clone(),
        Moveset = /* crear LearnedMove objects */,
    };
    
    if (_editingPosition == 0)
    {
        // Nuevo Pokemon
        var result = TeamService.AddPokemon(pokemon);
        if (!result.success)
        {
            ShowError(result.error);
        }
    }
    else
    {
        // Actualizar existente
        TeamService.UpdatePokemon(_editingPosition, pokemon);
    }
    
    _showPokemonEditor = false;
}
```

---

## Battle Assistant Window

### Propósito
Mostrar información del oponente y recomendaciones de batalla (Fase futura).

### Servicios a Usar (cuando esté implementado)
- `BattleDetector.CurrentBattle` - Estado actual de batalla
- `BattleAdvisor.RecommendMove()` - Mejor movimiento
- `BattleAdvisor.RecommendSwitch()` - Recomendación de cambio
- `DamageCalculator.CalculateDamage()` - Daño estimado

---

## Configuration Window

### Extender la ventana actual

```csharp
// En tu ventana de configuración existente, agrega:

ImGui.SeparatorText("Team Settings");

if (ImGui.Button("Open Team Manager"))
{
    _showTeamManager = true;
}

ImGui.Text($"Current Team Size: {TeamService.TeamSize}/6");

// Auto-save settings al cambiar
if (ImGui.Checkbox("Enable Battle Assistant", ref ConfigService.Settings.Battle.EnableBattleAssistant))
{
    ConfigService.SaveSettings();
}

if (ImGui.Checkbox("Auto-detect Battle", ref ConfigService.Settings.Battle.AutoDetectBattle))
{
    ConfigService.SaveSettings();
}
```

---

## Ejemplos Completos

### Inicialización Completa

```csharp
public class MyOverlay : Overlay
{
    private bool _showTeamManager = false;
    private bool _showPokemonEditor = false;
    
    public MyOverlay() : base("PokeMMOHelper")
    {
        // Inicializar servicios
        ConfigService.Initialize();
        
        // Cargar último equipo (opcional)
        var teams = ConfigService.GetSavedTeams();
        if (teams.Count > 0)
        {
            ConfigService.LoadTeam(teams[0]);
        }
    }
    
    protected override void Render()
    {
        // Renderizar ventanas
        RenderTeamManager();
        RenderPokemonEditor();
        RenderBattleInfo(); // Tu ventana existente
        RenderConfiguration(); // Tu ventana existente
    }
}
```

### Workflow Típico del Usuario

1. Usuario abre Team Manager
2. Hace clic en "Add" en un slot vacío
3. Se abre Pokemon Editor
4. Usuario busca "Charizard" (usando PokemonService.GetPokemon())
5. Configura IVs, EVs, Nature
6. Stats se calculan automáticamente (StatsCalculator)
7. Selecciona 4 movimientos
8. Hace clic en "Save"
9. Pokemon se agrega al equipo (TeamService.AddPokemon())
10. Usuario guarda el equipo completo (ConfigService.SaveTeam("Main Team"))

---

## Notas Importantes

### Thread Safety
Todos los servicios son thread-safe. Puedes llamarlos desde cualquier thread.

### Performance
- `CalculateFinalStats()` es rápido (~0.1ms), puedes llamarlo cada frame
- `PokemonService.GetPokemon()` hace API call, usa async/await y caché

### Validación
Siempre valida antes de agregar Pokemon:
```csharp
var (success, error) = TeamService.AddPokemon(pokemon);
if (!success)
{
    // Mostrar error al usuario
    ImGui.TextColored(new Vector4(1, 0, 0, 1), error);
}
```

### Persistencia
- Settings se guardan en `data/config/app_settings.json`
- Teams se guardan en `data/teams/{nombre}.json`
- Auto-save settings al cambiar configuración
- Guarda equipos manualmente con botón "Save"

---

## Próximos Pasos

1. Implementa Team Manager window
2. Implementa Pokemon Editor dialog
3. Prueba guardar/cargar equipos
4. Integra con tu ventana de batalla existente
5. (Futuro) Implementa Battle Assistant con recomendaciones

¿Questions? Consulta la API Reference para detalles completos de cada servicio.
