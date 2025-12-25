using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// Handles pre-loading of ALL API data at startup
/// </summary>
public static class DataLoader
{
    public static bool IsLoaded { get; private set; } = false;
    public static bool IsBackgroundLoadComplete { get; private set; } = false;
    public static string LoadingStatus { get; private set; } = "";
    public static float LoadingProgress { get; private set; } = 0f;
    public static string? ErrorMessage { get; private set; } = null;

    // Background loading tracking
    public static int TotalPokemonToLoad { get; private set; } = 0;
    public static int PokemonLoaded { get; private set; } = 0;
    public static DateTime BackgroundLoadStartTime { get; private set; }

    /// <summary>
    /// Loads ALL data from APIs at startup (complete pre-loading)
    /// DEPRECATED: Use Load MinimalData() + LoadBackgroundData() instead
    /// </summary>
    public static async Task LoadAllData()
    {
        LoadingProgress = 0f;
        ErrorMessage = null;

        try
        {
            // Try loading from disk cache first
            LoadingStatus = "Checking cache...";
            var cache = CacheManager.LoadCache();

            if (cache != null && CacheManager.IsCacheValid(cache))
            {
                LoadingStatus = "Loading from cache...";
                LoadingProgress = 0.1f;

                PokemonService.LoadFromCache(cache);

                LoadingStatus = "Ready!";
                LoadingProgress = 1.0f;
                IsLoaded = true;

                Console.WriteLine("Loaded all data from cache (fast!)");
                return; // Skip API loading
            }

            Console.WriteLine("Cache invalid or not found, loading from API...");

            // Cache invalid or doesn't exist, load from API
            // Phase 1: Pokemon list (1302)
            Console.WriteLine("[DATALOADER] Phase 1 START: Loading Pokemon list...");
            LoadingStatus = "Loading Pokemon list...";
            LoadingProgress = 0.02f;
            await PokemonService.GetPokemonList();
            Console.WriteLine("[DATALOADER] Phase 1 COMPLETE");
            LoadingProgress =

 0.05f;

            // Phase 2: Items list (2180)
            Console.WriteLine("[DATALOADER] Phase 2 START: Loading items list...");
            LoadingStatus = "Loading items list...";
            LoadingProgress = 0.06f;
            await PokemonService.GetAllItems();
            Console.WriteLine("[DATALOADER] Phase 2 COMPLETE");
            LoadingProgress = 0.10f;

            // Phase 3: Abilities list (300)
            Console.WriteLine("[DATALOADER] Phase 3 START: Loading abilities list...");
            LoadingStatus = "Loading abilities list...";
            LoadingProgress = 0.11f;
            await PokemonService.GetAllAbilities();
            Console.WriteLine("[DATALOADER] Phase 3 COMPLETE");
            LoadingProgress = 0.15f;

            // ⭐ Phase 4: ALL POKEMON with Complete Movesets (1302 Pokemon - LONGEST PHASE)
            Console.WriteLine("[DATALOADER] Phase 4 START: Loading ALL 1302 Pokemon with moves...");
            LoadingStatus = "Loading ALL 1302 Pokemon with moves (45-60 minutes first time)...";
            LoadingProgress = 0.15f;
            await PokemonService.PreloadAllPokemon();
            Console.WriteLine("[DATALOADER] Phase 4 COMPLETE");
            LoadingProgress = 0.50f;  // This is the bulk of the work!

            // Phase 5: ALL Item Details (2180 API calls)
            Console.WriteLine("[DATALOADER] Phase 5 START: Loading item details...");
            LoadingStatus = "Loading ALL item details...";
            LoadingProgress = 0.55f;
            await PokemonService.PreloadAllItemDetails();
            Console.WriteLine("[DATALOADER] Phase 5 COMPLETE");
            LoadingProgress = 0.75f;

            // Phase 6: ALL Move Details (900+ API calls)
            Console.WriteLine("[DATALOADER] Phase 6 START: Loading move details...");
            LoadingStatus = "Loading ALL move details...";
            LoadingProgress = 0.80f;
            await PokemonService.PreloadAllMoveDetails();
            Console.WriteLine("[DATALOADER] Phase 6 COMPLETE");
            LoadingProgress = 0.92f;

            // Phase 7: ALL Ability Details (300 API calls)
            Console.WriteLine("[DATALOADER] Phase 7 START: Loading ability details...");
            LoadingStatus = "Loading ALL ability details...";
            LoadingProgress = 0.95f;
            await PokemonService.PreloadAllAbilityDetails();
            Console.WriteLine("[DATALOADER] Phase 7 COMPLETE");
            LoadingProgress = 0.98f;

            // Save cache to disk for next time
            Console.WriteLine("[DATALOADER] Saving cache to disk...");
            LoadingStatus = "Saving cache to disk...";
            var cacheData = PokemonService.GetCacheData();
            CacheManager.SaveCache(cacheData);
            Console.WriteLine("[DATALOADER] Cache saved successfully");

            LoadingStatus = "Ready!";
            LoadingProgress = 1.0f;
            IsLoaded = true;

            Console.WriteLine("Loaded all data from API and saved to cache");
        }
        catch (Exception ex)
        {
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine("[DATALOADER] *** EXCEPTION CAUGHT ***");
            Console.WriteLine($"[DATALOADER] Type: {ex.GetType().Name}");
            Console.WriteLine($"[DATALOADER] Message: {ex.Message}");
            Console.WriteLine($"[DATALOADER] Stack Trace:");
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[DATALOADER] Inner Exception: {ex.InnerException.Message}");
                Console.WriteLine($"[DATALOADER] Inner Stack:");
                Console.WriteLine(ex.InnerException.StackTrace);
            }
            Console.WriteLine("=".PadRight(50, '='));

            LoadingStatus = "Error loading data";
            ErrorMessage = ex.Message;
            LoadingProgress = 0f;
            IsLoaded = false;
        }
    }

    /// <summary>
    /// Fast minimal data load for app startup (~5 seconds)
    /// </summary>
    public static async Task LoadMinimalData()
    {
        LoadingProgress = 0f;
        ErrorMessage = null;

        try
        {
            Console.WriteLine("[DATALOADER] === MINIMAL LOAD START ===");

            // Try loading from disk cache first
            LoadingStatus = "Checking cache...";
            var cache = CacheManager.LoadCache();

            if (cache != null && CacheManager.IsCacheValid(cache))
            {
                Console.WriteLine("[DATALOADER] Cache valid, loading from cache");
                LoadingStatus = "Loading from cache...";
                LoadingProgress = 0.1f;

                PokemonService.LoadFromCache(cache);

                LoadingStatus = "Ready!";
                LoadingProgress = 1.0f;
                IsLoaded = true;
                IsBackgroundLoadComplete = true; // Cache means everything is loaded

                Console.WriteLine($"[DATALOADER] Loaded from cache - Background load not needed");
                return;
            }

            Console.WriteLine("[DATALOADER] No valid cache, loading minimal data...");

            // Phase 1: Pokemon list (fast)
            Console.WriteLine("[DATALOADER] Phase 1: Loading Pokemon list...");
            LoadingStatus = "Loading Pokemon list...";
            LoadingProgress = 0.3f;
            await PokemonService.GetPokemonList();

            // Phase 2: Items list (fast)
            Console.WriteLine("[DATALOADER] Phase 2: Loading items list...");
            LoadingStatus = "Loading items list...";
            LoadingProgress = 0.6f;
            await PokemonService.GetAllItems();

            // Phase 3: Abilities list (fast)
            Console.WriteLine("[DATALOADER] Phase 3: Loading abilities list...");
            LoadingStatus = "Loading abilities list...";
            LoadingProgress = 0.9f;
            await PokemonService.GetAllAbilities();

            LoadingStatus = "Ready!";
            LoadingProgress = 1.0f;
            IsLoaded = true;

            Console.WriteLine("[DATALOADER] === MINIMAL LOAD COMPLETE ===");
            Console.WriteLine("[DATALOADER] App ready - background loading will continue");
        }
        catch (Exception ex)
        {
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine("[DATALOADER] *** EXCEPTION in LoadMinimalData ***");
            Console.WriteLine($"[DATALOADER] Message: {ex.Message}");
            Console.WriteLine($"[DATALOADER] Stack: {ex.StackTrace}");
            Console.WriteLine("=".PadRight(50, '='));

            LoadingStatus = "Error loading data";
            ErrorMessage = ex.Message;
            LoadingProgress = 0f;
            IsLoaded = false;
        }
    }

    /// <summary>
    /// Background heavy data loading (Pokemon, Items, Moves, Abilities)
    /// </summary>
    public static async Task LoadBackgroundData(IProgress<LoadingProgress>? progress = null)
    {
        if (IsBackgroundLoadComplete)
        {
            Console.WriteLine("[DATALOADER] Background load already complete");
            return;
        }

        BackgroundLoadStartTime = DateTime.Now;
        Console.WriteLine("[DATALOADER] === BACKGROUND LOAD START ===");

        try
        {
            // Phase 4: ALL POKEMON (longest phase)
            Console.WriteLine("[DATALOADER] Background Phase 1: Loading ALL Pokemon...");
            await PokemonService.PreloadAllPokemon(progress);

            // Phase 5: Item details
            Console.WriteLine("[DATALOADER] Background Phase 2: Loading item details...");
            await PokemonService.PreloadAllItemDetails();

            // Phase 6: Move details
            Console.WriteLine("[DATALOADER] Background Phase 3: Loading move details...");
            await PokemonService.PreloadAllMoveDetails();

            // Phase 7: Ability details
            Console.WriteLine("[DATALOADER] Background Phase 4: Loading ability details...");
            await PokemonService.PreloadAllAbilityDetails();

            // Save cache
            Console.WriteLine("[DATALOADER] Saving cache to disk...");
            var cacheData = PokemonService.GetCacheData();
            CacheManager.SaveCache(cacheData);
            Console.WriteLine("[DATALOADER] Cache saved successfully");

            IsBackgroundLoadComplete = true;
            var elapsed = DateTime.Now - BackgroundLoadStartTime;
            Console.WriteLine($"[DATALOADER] === BACKGROUND LOAD COMPLETE === ({elapsed.TotalMinutes:F1} minutes)");
        }
        catch (Exception ex)
        {
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine("[DATALOADER] *** EXCEPTION in LoadBackgroundData ***");
            Console.WriteLine($"[DATALOADER] Message: {ex.Message}");
            Console.WriteLine($"[DATALOADER] Stack: {ex.StackTrace}");
            Console.WriteLine("=".PadRight(50, '='));
        }
    }

    /// <summary>
    /// Resets loading state (for retry)
    /// </summary>
    public static void Reset()
    {
        IsLoaded = false;
        IsBackgroundLoadComplete = false;
        LoadingStatus = "";
        LoadingProgress = 0f;
        ErrorMessage = null;
    }
}
