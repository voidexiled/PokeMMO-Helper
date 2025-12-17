using System.Numerics;
using System.Runtime.InteropServices;
using System.Timers;
using ClickableTransparentOverlay;

using ImGuiNET;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PasaporteFiller.core;
using PasaporteFiller.services;
using SixLabors.ImageSharp;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Timer = System.Timers.Timer;

namespace PasaporteFiller
{
    public class RenderPasaporte : Overlay
    {
        [StructLayout(LayoutKind.Sequential)]
        struct ScreenSize
        {
            public int Width;
            public int Height;
        }

        // Importa la función GetSystemMetrics
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int nKey);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // Declara la función GetCursorPos de user32.dll
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        
        // Constantes para GetSystemMetrics
        const int SM_CXSCREEN = 0;
        const int SM_CYSCREEN = 1;

        // Obtiene el ancho y alto de la pantalla
        static readonly int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        static readonly int screenHeight = GetSystemMetrics(SM_CYSCREEN) - 50;
    
        Vector2 centerOfScreenView = new(screenWidth * 0.5f, screenHeight * 0.5f);

        private string fontPath = @"C:\Windows\Fonts\SegoeUI.ttf";

        static DateTime _today = DateTime.Now;
        static int year = _today.Year;
        static int month = _today.Month;
        static int day = _today.Day;
        public string fechahoy = $"{day}/{month}/{year}";

        private IntPtr Type1ImageHandle;
        private IntPtr Type2ImageHandle;

        private uint Type1ImageWidth;
        private uint Type1ImageHeight;
        
        private uint Type2ImageWidth;
        private uint Type2ImageHeight;

        private IntPtr BugTypeImageHandle;
        private IntPtr DarkTypeImageHandle;
        private IntPtr DragonTypeImageHandle;
        private IntPtr ElectricTypeImageHandle;
        private IntPtr FairyTypeImageHandle;
        private IntPtr FightingTypeImageHandle;
        private IntPtr FireTypeImageHandle;
        private IntPtr FlyingTypeImageHandle;
        private IntPtr GhostTypeImageHandle;
        private IntPtr GrassTypeImageHandle;
        private IntPtr GroundTypeImageHandle;
        private IntPtr IceTypeImageHandle;
        private IntPtr NormalTypeImageHandle;
        private IntPtr PoisonTypeImageHandle;
        private IntPtr PsychicTypeImageHandle;
        private IntPtr RockTypeImageHandle;
        private IntPtr SteelTypeImageHandle;
        private IntPtr WaterTypeImageHandle;

        private static bool transparentBackground = false;
        
        private static float _opacity = 0.95f;
        private static void ImGuiStyle_Red()
        {
            // Moonlight styleMadam-Herta from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = _opacity;
            style.DisabledAlpha = 1.0f;
            style.WindowPadding = new Vector2(12.0f, 12.0f);
            style.WindowRounding = 4.0f;
            style.WindowBorderSize = 0.0f;
            style.WindowMinSize = new Vector2(160f,20f);
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Right;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4f, 2f);
            style.FrameRounding = 2f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(6f, 2f);
            style.ItemInnerSpacing = new Vector2(2f,4f);
            style.CellPadding = new Vector2(12.10000038146973f, 9.199999809265137f);
            style.IndentSpacing = 6f;
            style.ColumnsMinSpacing = 50f;
            style.ScrollbarSize = 12f;
            style.ScrollbarRounding = 16f;
            style.GrabMinSize = 14f;
            style.GrabRounding = 16.0f;
            style.TabRounding = 0.0f;
            style.TabBorderSize = 0.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);
            style.SeparatorTextAlign = new Vector2(0.5f, 0.5f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.13f, 0.14f, 0.17f, 0.95f);
            if (transparentBackground)
            {
                style.Colors[(int)ImGuiCol.Text] = new Vector4(0.86f, 0.93f, 0.89f, 1.0f);
                style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.86f, 0.93f, 0.89f, 0.4f);                
            }
            else
            {
                style.Colors[(int)ImGuiCol.Text]= new Vector4(0.86f, 0.93f, 0.89f, 0.75f);
                style.Colors[(int)ImGuiCol.TextDisabled]= new Vector4(0.86f, 0.93f, 0.89f, 0.27f);
            }
            style.Colors[(int)ImGuiCol.Border]                = new Vector4(0.31f, 0.31f, 1.00f, 0.00f);
            style.Colors[(int)ImGuiCol.BorderShadow]          = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            style.Colors[(int)ImGuiCol.FrameBg]               = new Vector4(0.20f, 0.22f, 0.27f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered]        = new Vector4(0.92f, 0.18f, 0.29f, 0.78f);
            style.Colors[(int)ImGuiCol.FrameBgActive]         = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.TitleBg]               = new Vector4(0.20f, 0.22f, 0.27f, 1.00f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed]      = new Vector4(0.20f, 0.22f, 0.27f, 0.75f);
            style.Colors[(int)ImGuiCol.TitleBgActive]         = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.MenuBarBg]             = new Vector4(0.20f, 0.22f, 0.27f, 0.47f);
            style.Colors[(int)ImGuiCol.ScrollbarBg]           = new Vector4(0.20f, 0.22f, 0.27f, 1.00f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab]         = new Vector4(0.09f, 0.15f, 0.16f, 1.00f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered]  = new Vector4(0.92f, 0.18f, 0.29f, 0.78f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive]   = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.CheckMark]             = new Vector4(0.71f, 0.22f, 0.27f, 1.00f);
            style.Colors[(int)ImGuiCol.SliderGrab]            = new Vector4(0.47f, 0.77f, 0.83f, 0.14f);
            style.Colors[(int)ImGuiCol.SliderGrabActive]      = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.Button]                = new Vector4(0.47f, 0.77f, 0.83f, 0.14f);
            style.Colors[(int)ImGuiCol.ButtonHovered]         = new Vector4(0.92f, 0.18f, 0.29f, 0.86f);
            style.Colors[(int)ImGuiCol.ButtonActive]          = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.Header]                = new Vector4(0.92f, 0.18f, 0.29f, 0.76f);
            style.Colors[(int)ImGuiCol.HeaderHovered]         = new Vector4(0.92f, 0.18f, 0.29f, 0.86f);
            style.Colors[(int)ImGuiCol.HeaderActive]          = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.Separator]             = new Vector4(0.14f, 0.16f, 0.19f, 1.00f);
            style.Colors[(int)ImGuiCol.SeparatorHovered]      = new Vector4(0.92f, 0.18f, 0.29f, 0.78f);
            style.Colors[(int)ImGuiCol.SeparatorActive]       = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.ResizeGrip]            = new Vector4(0.47f, 0.77f, 0.83f, 0.04f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered]     = new Vector4(0.92f, 0.18f, 0.29f, 0.78f);
            style.Colors[(int)ImGuiCol.ResizeGripActive]      = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.PlotLines]             = new Vector4(0.86f, 0.93f, 0.89f, 0.63f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered]      = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.PlotHistogram]         = new Vector4(0.86f, 0.93f, 0.89f, 0.63f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered]  = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.TextSelectedBg]        = new Vector4(0.92f, 0.18f, 0.29f, 0.43f);
            style.Colors[(int)ImGuiCol.PopupBg]               = new Vector4(0.20f, 0.22f, 0.27f, 0.9f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg]      = new Vector4(0.20f, 0.22f, 0.27f, 0.73f);

        }
        private void ConfigureOverlaySize()
        {
            if (this.Size.Width == screenWidth && this.Size.Height == screenHeight) return;
            this.Size = new Size(screenWidth, screenHeight);
            this.Position = new Point(0, 0);
        }


        private static string currentDirectory = Directory.GetCurrentDirectory();
        private static string typesImageDirectory = $"{currentDirectory}/images/types";
        private void LoadImages()
        {
            //var currentDirectory = Directory.GetCurrentDirectory();
            //var typesImageDirectory = $"{currentDirectory}/images/types";

            var typesFolderExists = Directory.Exists(typesImageDirectory);
            if (!typesFolderExists)
            {
                Directory.CreateDirectory(typesImageDirectory);
            }

            // Descargar imagenes si no existen
            if (Directory.GetFiles(typesImageDirectory).Length < 18)
            {
                // var types = PokemonService.GetPokemonTypes().Result;
                // foreach (var type in types)
                // {
                //     var imageUrl = $"{PokemonService.POKEMON_IMAGE_URL}{type.Name}.svg";
                //     var image = Image.Load<Rgba32>(new WebClient().DownloadData(imageUrl));
                //     image.Save($"{currentDirectory}/images/types/{type.Name}.png");
                // }
            }
            
            AddOrGetImagePointer($"{typesImageDirectory}/bug.png", true, out BugTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/dark.png", true, out DarkTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/dragon.png", true, out DragonTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/electric.png", true, out ElectricTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/fairy.png", true, out FairyTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/fighting.png", true, out FightingTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/fire.png", true, out FireTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/flying.png", true, out FlyingTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/ghost.png", true, out GhostTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/grass.png", true, out GrassTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/ground.png", true, out GroundTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/ice.png", true, out IceTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/normal.png", true, out NormalTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/poison.png", true, out PoisonTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/psychic.png", true, out PsychicTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/rock.png", true, out RockTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/steel.png", true, out SteelTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/water.png", true, out WaterTypeImageHandle, out _, out _);
            

        }
        
        
        void UpdateTypesImageHandles(List<PokemonType> types)
        {
            try
            {
                
                if (types.Count <= 0)
                {
                    Type1ImageHandle = IntPtr.Zero;
                    Type2ImageHandle = IntPtr.Zero;
                    Console.WriteLine("No hay tipos");
                    return;
                }
             
                AddOrGetImagePointer($"{typesImageDirectory}/{types[0].Name}.png", true, out Type1ImageHandle, out Type1ImageWidth, out Type1ImageHeight);
                Console.WriteLine($"Type1ImageHandle: {Type1ImageHandle}");
                if (types.Count > 1)
                {
                    AddOrGetImagePointer($"{typesImageDirectory}/{types[1].Name}.png", true, out Type2ImageHandle, out Type2ImageWidth, out Type2ImageHeight);
                    Console.WriteLine($"Type2ImageHandle: {Type2ImageHandle}");
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void SearchPokemon()
        {
            
            PokemonService.GetPokemon(PokemonNameSearch).ContinueWith(task =>
            {
                if (task.Result == null)
                {
                    _currentPokemon = null;
                    UpdateTypesImageHandles(new List<PokemonType>());
                    return;
                }
                    
                UpdateTypesImageHandles(task.Result.Types);
                
                _currentPokemon = task.Result;
                Console.WriteLine(JObject.FromObject(_currentPokemon));
            });
        }

        private void RenderTypeImageTooltip(string tooltip, Vector4 tooltipColor)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                //ImGui.Image(imageHandle, imageSize);
                ImGui.TextColored(tooltipColor, tooltip);
                ImGui.EndTooltip();
            }
        }
        
        
        private void RenderTypesImages(List<string> typesNames)
        {
            var typesCount = typesNames.Count;
            foreach (var typeName in typesNames)
                switch (typeName)
                {
                    case "bug":
                        ImGui.Image(BugTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Bug / Bicho / Insecto", new Vector4(0.639f, 0.831f, 0.224f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "dark":
                        ImGui.Image(DarkTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Dark / Oscuro / Siniestro", new Vector4(0.451f, 0.314f, 0.275f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "dragon":
                        ImGui.Image(DragonTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Dragon / Dragón", new Vector4(0.451f, 0.306f, 0.831f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "electric":
                        ImGui.Image(ElectricTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Electric / Eléctrico", new Vector4(0.980f, 0.831f, 0.173f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "fairy":
                        ImGui.Image(FairyTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Fairy / Hada", new Vector4(0.933f, 0.561f, 0.757f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "fighting":
                        ImGui.Image(FightingTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Fighting / Lucha", new Vector4(0.784f, 0.157f, 0.282f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "fire":
                        ImGui.Image(FireTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Fire / Fuego", new Vector4(0.933f, 0.306f, 0.247f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "flying":
                        ImGui.Image(FlyingTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Flying / Volador", new Vector4(0.659f, 0.596f, 0.976f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "ghost":
                        ImGui.Image(GhostTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Ghost / Fantasma", new Vector4(0.439f, 0.306f, 0.475f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "grass":
                        ImGui.Image(GrassTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Grass / Planta", new Vector4(0.475f, 0.780f, 0.349f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "ground":
                        ImGui.Image(GroundTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Ground / Tierra", new Vector4(0.878f, 0.745f, 0.416f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "ice":
                        ImGui.Image(IceTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Ice / Hielo", new Vector4(0.529f, 0.847f, 0.847f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "normal":
                        ImGui.Image(NormalTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Normal / Normal", new Vector4(0.659f, 0.659f, 0.471f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "poison":
                        ImGui.Image(PoisonTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Poison / Veneno", new Vector4(0.639f, 0.306f, 0.639f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "psychic":
                        ImGui.Image(PsychicTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Psychic / Psíquico", new Vector4(0.976f, 0.306f, 0.494f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "rock":
                        ImGui.Image(RockTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Rock / Roca", new Vector4(0.722f, 0.624f, 0.286f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "steel":
                        ImGui.Image(SteelTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Steel / Acero", new Vector4(0.722f, 0.722f, 0.827f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                    case "water":
                        ImGui.Image(WaterTypeImageHandle, new Vector2(32, 32));
                        RenderTypeImageTooltip("Water / Agua", new Vector4(0.459f, 0.639f, 0.906f, 1.0f));
                        if (typesCount != typesNames.IndexOf(typeName)) ImGui.SameLine();
                        break;
                }
        }
        
        
        public string PokemonNameSearch = "";
        private bool _isOpenInfoWindow = true;
        private Pokemon? _currentPokemon = null;

        private bool _renderNormalWeaknesses = false;
        private bool _renderNormalStrengths = false;

        private bool _showConfiguration = false;
        
        
        /*
         * RENDER METHOD
         * STARTS HERE * <--------------------------------
         */
        
        private static bool _checkForPokemonInScreenEnabled = true;
        private static int _intervalCheckForPokemon = 5000;
        public  System.Timers.Timer timer = new(_intervalCheckForPokemon);
        
        public RenderPasaporte() : base("PasaporteFiller")
        {
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += ScreenTextRecognizer.CheckForPokemonName;
            timer.Start();
            
        }
        
        private static async void FillPokemonList()
        {
            var pokeList = await PokemonService.GetPokemonList();
            ScreenTextRecognizer.PokemonNames = pokeList;
        }


        private bool _showWeaknessSearchedPokemon = true;
        private bool _showStrenghtsSearchedPokemon = false;
        
        private bool _showAreaOfSearchVision = false;

        private bool _selectingAreaFirstPoint = false;
        private bool _selectingAreaSecondPoint = false;
        
        private int _firstPointX;
        private int _firstPointY;
        
        private int _secondPointX;
        private int _secondPointY;

        public static bool _useLevenshteinMethod = true;
        private bool _showLogWindow = false;
        
        protected override void Render()
        {
            FillPokemonList();
            ConfigureOverlaySize();
            ImGuiStyle_Red();
            LoadImages();
            
            //Console.WriteLine(ScreenTextRecognizer.PokemonNames.Count);
        
            ReplaceFont(fontPath, 18, FontGlyphRangeType.Japanese);
            

            if (_checkForPokemonInScreenEnabled){
                if (ScreenTextRecognizer.LastRecognizedPokemonName != PokemonNameSearch)
                {
                    PokemonNameSearch = ScreenTextRecognizer.LastRecognizedPokemonName;
                    SearchPokemon();
                }
            }
            
            ImGui.Begin("Busqueda",  ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize);
            ImGui.Checkbox("Mostrar Configuración", ref _showConfiguration);
            ImGui.Spacing();
            ImGui.Checkbox("Debilidades", ref _showWeaknessSearchedPokemon);
            ImGui.SameLine();
            ImGui.Checkbox("Fortalezas", ref _showStrenghtsSearchedPokemon);
            
            ImGui.SeparatorText("Busqueda de Pokemon");

            ImGui.SetNextItemWidth(140f);
            if (ImGui.IsKeyReleased(ImGuiKey.F4)) ImGui.SetKeyboardFocusHere();
            if (ImGui.InputText("", ref PokemonNameSearch, 40,
                    ImGuiInputTextFlags.EscapeClearsAll | ImGuiInputTextFlags.EnterReturnsTrue)) SearchPokemon();
            
            if (ImGui.Button("Enviar")) SearchPokemon();
            if (ImGui.IsKeyReleased(ImGuiKey.Enter) ) SearchPokemon();

            ImGui.End();
            ImGui.Begin("Información Pokémon", ref _isOpenInfoWindow,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize );
            if (_currentPokemon == null)
            {
                ImGui.SeparatorText("No especificado...");
            }
            else
            {
                var normalizedPokemonName = _currentPokemon.Name.ToUpper()[0] + _currentPokemon.Name[1..];
                ImGui.SeparatorText(normalizedPokemonName);
                
                if (_currentPokemon.Types.Count > 0)
                {
                    
                    ImGui.Image(Type1ImageHandle, new Vector2(Type1ImageWidth/4, Type1ImageHeight/4));
                    
                }

                if (_currentPokemon.Types.Count > 1)
                {
                    ImGui.SameLine();
                    ImGui.Image(Type2ImageHandle, new Vector2(Type2ImageWidth / 4, Type2ImageHeight / 4));
                }

                if (_showWeaknessSearchedPokemon)
                {
                    
                    ImGui.SeparatorText("Debilidades");

                    if (_currentPokemon.DoubleDamageFrom.Count > 0)
                    {
                        ImGui.Text("Recibe x2 de:");
                        RenderTypesImages(_currentPokemon.DoubleDamageFrom);
                        ImGui.Spacing();
                    }
                    if (_renderNormalWeaknesses)
                    {
                        if (_currentPokemon.NormalDamageFrom.Count > 0)
                        {
                            ImGui.Text("Recibe x1 de:");
                            RenderTypesImages(_currentPokemon.NormalDamageFrom);
                            ImGui.Spacing();
                        }
                    }

                    if (_currentPokemon.HalfDamageFrom.Count > 0)
                    {
                        ImGui.Text("Recibe x0.5 de:");
                        RenderTypesImages(_currentPokemon.HalfDamageFrom);
                        ImGui.Spacing();
                    }

                    if (_currentPokemon.NoDamageFrom.Count > 0)
                    {
                        ImGui.Text("Recibe x0 de:");
                        RenderTypesImages(_currentPokemon.NoDamageFrom);
                        ImGui.Spacing();
                    }
                }

                if (_showStrenghtsSearchedPokemon)
                {    
                    ImGui.SeparatorText("Fortalezas");
                
                    if (_currentPokemon.DoubleDamageTo.Count > 0)
                    {
                        ImGui.Text("Hace x2 a:");
                        RenderTypesImages(_currentPokemon.DoubleDamageTo);
                        ImGui.Spacing();
                    }

                    if (_renderNormalStrengths)
                    {
                        if (_currentPokemon.NormalDamageTo.Count > 0)
                        {
                            ImGui.Text("Hace x1 a:");
                            RenderTypesImages(_currentPokemon.NormalDamageTo);
                            ImGui.Spacing();
                        }
                    }

                    if (_currentPokemon.HalfDamageTo.Count > 0)
                    {
                        ImGui.Text("Hace x0.5 a:");
                        RenderTypesImages(_currentPokemon.HalfDamageTo);
                        ImGui.Spacing();
                    }

                    if (_currentPokemon.NoDamageTo.Count > 0)
                    {
                        ImGui.Text("Hace x0 a:");
                        RenderTypesImages(_currentPokemon.NoDamageTo);
                        ImGui.Spacing();
                    }
                }

            }
            
            ImGui.End();


            // Preview screenshoot
            if (false)
            {
//                AddOrGetImagePointer();
                ImGui.Begin("PreviewScreenshoot");
                ImGui.Image(ScreenTextRecognizer.PreviewScreenshootImage, new (320, 180));
                ImGui.End();
            }

            
            if (_showConfiguration)
            {
                ImGui.Begin("Configuración", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Modal);
                ImGui.SeparatorText("Información");
                ImGui.TextColored(new(0.0f, 0.5f, 0.0f, 1.0f), "Pokemon cargados  -  ");
                ImGui.SameLine();
                ImGui.TextColored(new(0.25f, 0.48f, 0.15f, 0.9f), $"{ScreenTextRecognizer.PokemonNames.Count}");
                ImGui.SeparatorText("Window");
                ImGui.Checkbox("Fondo transparente", ref transparentBackground);
                ImGui.Spacing();
                if (transparentBackground)
                {
                    ImGui.Text("Transparencia del fondo");
                    ImGui.SliderFloat("", ref _opacity, 0.0f, 1.0f);
                    ImGui.Spacing();
                }
                
                ImGui.SeparatorText("AI");
                ImGui.Checkbox("Buscar Pokemon en pantalla", ref _checkForPokemonInScreenEnabled);
                ImGui.Spacing();
                ImGui.Checkbox("Usar método de Levenshtein", ref _useLevenshteinMethod);
                if (_checkForPokemonInScreenEnabled)
                {
                    ImGui.Text("Intervalo de busqueda (ms)");
                    ImGui.SliderInt("", ref _intervalCheckForPokemon, 300, 15000);
                    timer.Enabled = true;
                    if (timer.Interval != _intervalCheckForPokemon)
                    {
                        timer.Interval = _intervalCheckForPokemon;
                    }
                    timer.Start();
                    ImGui.Spacing();
                    if (ImGui.Button("Seleccionar Punto 1")) 
                    {
                        _selectingAreaFirstPoint = true;
                        _selectingAreaSecondPoint = false;
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Button("Seleccionar Punto 2"))
                    {
                        _selectingAreaFirstPoint = false;
                        _selectingAreaSecondPoint = true;
                    }
                    ImGui.Spacing();
                    
                    
                    ImGui.BeginTable("##Positions", 4, ImGuiTableFlags.Borders );
                    ImGui.TableSetupColumn("X1", ImGuiTableColumnFlags.NoResize, 50);
                    ImGui.TableSetupColumn("Y1", ImGuiTableColumnFlags.NoResize, 50);
                    ImGui.TableSetupColumn("X2", ImGuiTableColumnFlags.NoResize, 50);
                    ImGui.TableSetupColumn("Y2", ImGuiTableColumnFlags.NoResize, 50);
                    ImGui.TableHeadersRow();
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text($"{_firstPointX}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{_firstPointY}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{_secondPointX}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{_secondPointY}");
                    ImGui.EndTable();
                    
                    ImGui.Spacing();

                    ImGui.Checkbox("Renderizar area de visión", ref _showAreaOfSearchVision);

                    if (_showAreaOfSearchVision)
                    {
                        var copyOfStyles = ImGuiNET.ImGui.GetStyle();
                        var editedStyles = ImGuiNET.ImGui.GetStyle();
                        
                        editedStyles.Colors[(int)ImGuiCol.WindowBg] = new Vector4(8.0f, 0.1f, 0.1f, 0.15f);
                        
                        ImGui.SetNextWindowPos(new(_firstPointX, _firstPointY));
                        ImGui.SetNextWindowSize(new(_secondPointX - _firstPointX, _secondPointY - _firstPointY));
                        ImGui.Begin("Area de visión", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
                        ImGui.End();
                        
                        editedStyles = copyOfStyles;
                    }
                    
                    
                    if (_selectingAreaFirstPoint || _selectingAreaSecondPoint)
                    {
                        //var mousePos = ImGui.GetMousePos();
                        POINT cursorPos;
                        GetCursorPos(out cursorPos);
                        ImGui.BeginTooltip();
                        ImGui.Text($"X: {cursorPos.X}");
                        ImGui.Text($"Y: {cursorPos.Y}");
                        ImGui.Spacing();
                        ImGui.Text("Haz click izquierdo para seleccionar");
                        ImGui.Text("Haz click derecho para cancelar");
                        ImGui.EndTooltip();
                        /////
                        const int leftMouseButton = 0x01;
                        const int rightMouseButton = 0x02;
                        if ((GetAsyncKeyState(leftMouseButton) & 0x8000) != 0)
                        {
                            if (_selectingAreaFirstPoint)
                            {
                                _firstPointX = cursorPos.X;
                                _firstPointY = cursorPos.Y;
                                _selectingAreaFirstPoint = false;
                            }
                            else
                            {
                                _secondPointX = cursorPos.X;
                                _secondPointY = cursorPos.Y;
                                _selectingAreaSecondPoint = false;
                            }
                            
                            if (_secondPointX < _firstPointX)
                                (_firstPointX, _secondPointX) = (_secondPointX, _firstPointX);
                            if (_secondPointY < _firstPointY)
                                (_firstPointY, _secondPointY) = (_secondPointY, _firstPointY);
                            
                            ScreenTextRecognizer.setArea(_firstPointX, _firstPointY, _secondPointX, _secondPointY);
                        } 
                        if ((GetAsyncKeyState(rightMouseButton) & 0x8000) != 0)
                        {
                            _selectingAreaFirstPoint = false;
                            _selectingAreaSecondPoint = false;
                        }                       
                    }
                }
                else
                {
                    timer.Enabled = false;
                    timer.Stop();
                }          
                
                ImGui.SeparatorText("Renderizado opcional");
                ImGui.Spacing();
                ImGui.Checkbox("X1 Debilidades", ref _renderNormalWeaknesses);
                ImGui.SameLine();
                ImGui.Checkbox("X1 Fortalezas", ref _renderNormalStrengths);

                
                
                ImGui.SeparatorText("Debug");
                ImGui.Checkbox("Logs Window", ref _showLogWindow);

                

                ImGui.End();
            }

            if (_showLogWindow)
            {            
                ImGui.Begin("Logs Window", ref _showLogWindow, ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Text($"Latest recognized text:{ScreenTextRecognizer.LastRecognizedText}");
                ImGui.Spacing();
                ImGui.Text($"Latest Levenshtein:{ScreenTextRecognizer.LastRecognizedPokemonNameWithLevenshtein}");
                ImGui.Spacing();
                ImGui.Text($"Latest Pokémon Name:{ScreenTextRecognizer.LastRecognizedPokemonName}");
                ImGui.End();
            }
            
        }
    }
}