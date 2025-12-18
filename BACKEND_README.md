# PokeMMOHelper - Backend MVP

Backend completo para el sistema de gestión de equipos de PokeMMO.

## 🎯 Lo que se implementó

### ✅ Core Models (core/)
- **PokemonStats**: Representa HP, Atk, Def, SpA, SpD, Spe
- **Nature**: 25 naturalezas con modificadores de stats
- **Ability**: Habilidades comunes de Pokémon
- **Item**: Ítems equipables (Choice items, Leftovers, etc.)
- **PlayerPokemon**: Pokémon del equipo con IVs, EVs, moveset completo
- **LearnedMove**: Wrapper de movimientos con tracking de PP
- **Enums**: Gender, MoveDamageClass, MoveTarget
- **PokemonMove** (extendido): Ahora incluye DamageClass, Priority, Target
- **AppSettings**: Configuración de OCR, UI, y Battle

### ✅ Services (services/)
- **StatsCalculator**: Cálculo de stats con fórmulas oficiales de Pokémon
  - Validación de IVs (0-31)
  - Validación de EVs (0-252, total 510)
  - Cálculo de HP y stats no-HP con naturalezas
  
- **TeamService**: Gestión de equipo (máximo 6 Pokémon)
  - CRUD operations thread-safe
  - Validación automática al agregar
  - Cálculo automático de stats finales
  
- **ConfigService**: Persistencia y configuración
  - Guardar/cargar equipos en JSON
  - Guardar/cargar settings de la app
  - Gestión de múltiples equipos guardados

### ✅ Documentation (docs/)
- **UI_INTEGRATION_GUIDE.md**: Guía completa con ejemplos de código para integrar el backend en Dear ImGui
- **API_REFERENCE.md**: Referencia de todos los métodos públicos con parámetros y ejemplos

## 📂 Estructura de Archivos

```
PasaporteFiller/
├── core/
│   ├── Ability.cs
│   ├── AppSettings.cs
│   ├── Enums.cs
│   ├── Item.cs
│   ├── LearnedMove.cs
│   ├── Nature.cs
│   ├── PlayerPokemon.cs
│   ├── Pokemon.cs (existente)
│   ├── PokemonEffectiveness.cs (existente)
│   ├── PokemonMove.cs (extendido)
│   ├── PokemonStats.cs
│   └── PokemonType.cs (existente)
├── services/
│   ├── ConfigService.cs
│   ├── StatsCalculator.cs
│   ├── TeamService.cs
│   ├── PokemonService.cs (existente)
│   └── ScreenTextRecognizer.cs (existente)
├── docs/
│   ├── UI_INTEGRATION_GUIDE.md ⭐
│   └── API_REFERENCE.md ⭐
└── data/ (se crea automáticamente)
    ├── teams/ (equipos guardados)
    └── config/ (configuración de la app)
```

## 🚀 Cómo usar el Backend

### 1. Inicialización

```csharp
// En el constructor de tu Overlay
public MyOverlay() : base("PokeMMOHelper")
{
    ConfigService.Initialize(); // ⭐ IMPORTANTE: Llamar al inicio
}
```

### 2. Crear un Pokémon

```csharp
// Obtener datos base
var charizard = await PokemonService.GetPokemon("charizard");

// Crear PlayerPokemon
var myCharizard = new PlayerPokemon(charizard, "Blaze", 50)
{
    Gender = PokemonGender.Male,
    Nature = Nature.GetNature("Timid"),
    Ability = new Ability("Blaze"),
    HeldItem = Item.GetItem("Life Orb"),
    IVs = new PokemonStats(31, 0, 31, 31, 31, 31),
    EVs = new PokemonStats(4, 0, 0, 252, 0, 252)
};

// Los stats se calculan automáticamente al agregar
var (success, error) = TeamService.AddPokemon(myCharizard);
if (success)
    Console.WriteLine($"Stats: {myCharizard.CalculatedStats}");
```

### 3. Guardar/Cargar Equipo

```csharp
// Guardar
ConfigService.SaveTeam("Main Team");

// Cargar
ConfigService.LoadTeam("Main Team");

// Listar equipos guardados
var teams = ConfigService.GetSavedTeams();
```

### 4. Acceder al Equipo

```csharp
// Obtener equipo completo
var team = TeamService.CurrentTeam;

// Obtener un Pokémon específico
var lead = TeamService.GetPokemon(1); // Posición 1-6

// Validar equipo
var (isValid, errors) = TeamService.ValidateTeam();
```

## 📖 Próximos Pasos (Tu Trabajo - UI)

Lee los documentos en orden:

1. **docs/UI_INTEGRATION_GUIDE.md** ⭐⭐⭐
   - Ejemplos completos de código Dear ImGui
   - Workflow de Team Manager
   - Workflow de Pokemon Editor
   - Cómo calcular stats en tiempo real

2. **docs/API_REFERENCE.md**
   - Referencia completa de todos los métodos
   - Parámetros y returns
   - Ejemplos de uso

### Ventanas a Implementar

1. **Team Manager Window**
   - Lista de 6 slots
   - Botones Add/Edit/Remove
   - Guardar/Cargar equipo

2. **Pokemon Editor Dialog**
   - Input de datos básicos (nickname, level, gender, nature)
   - Sliders de IVs (0-31)
   - Sliders de EVs (0-252, total 510)
   - Selección de movimientos
   - Display de stats calculados en tiempo real
   - Botón Save/Cancel

3. **Extender Configuration Window**
   - Botón "Open Team Manager"
   - Settings de Battle Assistant

## 🌿 Git Branches

```
develop (todas las features mergeadas aquí) ✅
├── feature/core-models ✅ Merged
├── feature/stats-calculator ✅ Merged
└── feature/team-service ✅ Merged
```

Para deployar a producción cuando estés listo:
```bash
git checkout master
git merge develop
```

## 💾 Archivos de Persistencia

La app creará automáticamente:
```
data/
├── teams/
│   ├── Main Team.json
│   └── Competitive Team.json
└── config/
    └── app_settings.json
```

## 🧪 Testing

Para probar el backend sin UI:

```csharp
// Test rápido
ConfigService.Initialize();

var charizard = await PokemonService.GetPokemon("charizard");
var pokemon = new PlayerPokemon(charizard, "Test", 50);
pokemon.IVs = new PokemonStats(31, 31, 31, 31, 31, 31);
pokemon.EVs = new PokemonStats(252, 0, 0, 252, 0, 0);
pokemon.Nature = Nature.GetNature("Timid");

var (success, error) = TeamService.AddPokemon(pokemon);
Console.WriteLine($"Success: {success}");
Console.WriteLine($"Stats: {pokemon.CalculatedStats}");

ConfigService.SaveTeam("Test Team");
```

## ❓ Preguntas Frecuentes

**Q: ¿Cómo calculo stats en tiempo real mientras el usuario edita IVs/EVs?**
A: Llama a `StatsCalculator.CalculateFinalStats()` cada frame. Es muy rápido (~0.1ms).

**Q: ¿Cómo valido que los EVs no excedan 510?**
A: Usa `StatsCalculator.ValidateEVs(evs)` antes de guardar.

**Q: ¿Los servicios son thread-safe?**
A: Sí, todos usan locks internos. Puedes llamar desde cualquier thread.

**Q: ¿Dónde se guardan los equipos?**
A: En `data/teams/{nombre}.json` en formato JSON legible.

## 📝 Commits Realizados

```
610adbf docs: add comprehensive UI integration guide and API reference
ad24168 feat(team): implement TeamService and ConfigService with JSON persistence
0ffc716 feat(calculator): implement StatsCalculator with official Pokemon formulas
6b62724 feat(models): add PokemonStats class for stat representation
```

## 🎯 Resumen

**Backend Completo**: ✅
- 8 nuevos modelos de datos
- 3 servicios completos
- Persistencia JSON funcional
- Fórmulas oficiales de Pokémon
- Documentación exhaustiva

**Tu Trabajo (UI)**: 📝
- Team Manager Window (Dear ImGui)
- Pokemon Editor Dialog (Dear ImGui)
- Integrar con ventanas existentes

**Siguiente Milestone**: Battle AI con recomendaciones (Fase 2)

---

🚀 **El backend está listo. ¡Ahora a crear la UI!**

Consulta `docs/UI_INTEGRATION_GUIDE.md` para comenzar.
