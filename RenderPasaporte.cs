using System.Numerics;
using System.Runtime.InteropServices;
using System.Timers;
using ClickableTransparentOverlay;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PasaporteFiller.core;
using PasaporteFiller.services;
using SharpGen.Runtime;
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
            style.WindowMinSize = new Vector2(160f, 20f);
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
            style.ItemInnerSpacing = new Vector2(2f, 4f);
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
                style.Colors[(int)ImGuiCol.Text] = new Vector4(0.86f, 0.93f, 0.89f, 0.75f);
                style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.86f, 0.93f, 0.89f, 0.27f);
            }

            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.31f, 0.31f, 1.00f, 0.00f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.20f, 0.22f, 0.27f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.92f, 0.18f, 0.29f, 0.78f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.20f, 0.22f, 0.27f, 1.00f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.20f, 0.22f, 0.27f, 0.75f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.20f, 0.22f, 0.27f, 0.47f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.20f, 0.22f, 0.27f, 1.00f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.09f, 0.15f, 0.16f, 1.00f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.92f, 0.18f, 0.29f, 0.78f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.71f, 0.22f, 0.27f, 1.00f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.47f, 0.77f, 0.83f, 0.14f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.47f, 0.77f, 0.83f, 0.14f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.92f, 0.18f, 0.29f, 0.86f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.92f, 0.18f, 0.29f, 0.76f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.92f, 0.18f, 0.29f, 0.86f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.14f, 0.16f, 0.19f, 1.00f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.92f, 0.18f, 0.29f, 0.78f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.47f, 0.77f, 0.83f, 0.04f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.92f, 0.18f, 0.29f, 0.78f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.86f, 0.93f, 0.89f, 0.63f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.86f, 0.93f, 0.89f, 0.63f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.92f, 0.18f, 0.29f, 1.00f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.92f, 0.18f, 0.29f, 0.43f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.20f, 0.22f, 0.27f, 0.9f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.20f, 0.22f, 0.27f, 0.73f);
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
            AddOrGetImagePointer($"{typesImageDirectory}/electric.png", true, out ElectricTypeImageHandle, out _,
                out _);
            AddOrGetImagePointer($"{typesImageDirectory}/fairy.png", true, out FairyTypeImageHandle, out _, out _);
            AddOrGetImagePointer($"{typesImageDirectory}/fighting.png", true, out FightingTypeImageHandle, out _,
                out _);
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

                AddOrGetImagePointer($"{typesImageDirectory}/{types[0].Name}.png", true, out Type1ImageHandle,
                    out Type1ImageWidth, out Type1ImageHeight);
                Console.WriteLine($"Type1ImageHandle: {Type1ImageHandle}");
                if (types.Count > 1)
                {
                    AddOrGetImagePointer($"{typesImageDirectory}/{types[1].Name}.png", true, out Type2ImageHandle,
                        out Type2ImageWidth, out Type2ImageHeight);
                    Console.WriteLine($"Type2ImageHandle: {Type2ImageHandle}");
                }
            }
            catch (Exception e)
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

        private string _currentTeamName = "";
        private string _teamComboPreview = "";


        private bool _renderNormalWeaknesses = false;
        private bool _renderNormalStrengths = false;

        private bool _showConfigurationWindow = false;
        private bool _showTeamManagerWindow = false;
        private bool _showPokemonEditorWindow = false;

        private Pokemon? _editedBasePokemon = null;
        private int _editingPosition = 0; // 0 = nuevo, 1-6 = editar existente

        // Pokemon Data
        private string _editedPokemonName = "";
        private string _editedNickname = "";
        private int _editedLevel = 50;
        private int _editedGenderIndex = 0; // 0 - Male | 1 - Female | 2 - Genderless
        private int _editedNatureIndex = 0;
        private int _editedAbilityIndex = 0;
        private int _editedItemIndex = 0;

        // Stats
        private int _editedIVsHP = 31;
        private int _editedIVsATTACK = 31;
        private int _editedIVsDEFENSE = 31;
        private int _editedIVsSPATTACK = 31;
        private int _editedIVsSPDEFENSE = 31;
        private int _editedIVsSPEED = 31;

        private int _editedEVsHP = 0;
        private int _editedEVsATTACK = 0;
        private int _editedEVsDEFENSE = 0;
        private int _editedEVsSPATTACK = 0;
        private int _editedEVsSPDEFENSE = 0;
        private int _editedEVsSPEED = 0;


        private PokemonStats _editedIvs = new(31, 31, 31, 31, 31, 31);
        private PokemonStats _editedEvs = new PokemonStats();
        private PokemonStats _editedCalculatedStats = new PokemonStats();

        // Moveset
        private List<LearnedMove> _selectedMoves = new();

        private bool _cancelled = false;
        private bool _confirmed = false;
        private bool _askConfirmReplace = false;


        /*
         * RENDER METHOD
         * STARTS HERE * <--------------------------------
         */

        private static bool _checkForPokemonInScreenEnabled = true;
        private static int _intervalCheckForPokemon = 5000;
        public System.Timers.Timer timer = new(_intervalCheckForPokemon);

        private void CalculateStatsRealTime()
        {
            if (_editedBasePokemon == null) return;

            var natures = Nature.GetAllNatures();
            var nature = natures[_editedNatureIndex];

            // Validar antes de calcular
            var evValidation = StatsCalculator.ValidateEVs(_editedEvs);
            var ivValidation = StatsCalculator.ValidateIVs(_editedIvs);

            if (evValidation.isValid && ivValidation.isValid)
            {
                _editedCalculatedStats = StatsCalculator.CalculateFinalStats(
                    _editedBasePokemon.BaseStats,
                    _editedIvs,
                    _editedEvs,
                    nature,
                    _editedLevel
                );
            }
        }

        void SavePokemonEditor()
        {
            var pokemon = new PlayerPokemon
            {
                Nickname = _editedNickname,
                BaseData = _editedBasePokemon,
                Level = _editedLevel,
                Gender = (PokemonGender)_editedGenderIndex,
                Nature = Nature.GetAllNatures()[_editedNatureIndex],
                Ability = Ability.GetCommonAbilities()[_editedAbilityIndex],
                HeldItem = Item.GetCommonItems()[_editedItemIndex],
                IVs = _editedIvs.Clone(),
                EVs = _editedEvs.Clone(),
                BaseStats = _editedBasePokemon.BaseStats.Clone(),
                Moveset = new List<LearnedMove>() /* crear LearnedMove objects */,
            };

            if (_editingPosition == 0)
            {
                // Nuevo Pokemon
                var result = TeamService.AddPokemon(pokemon);
                if (!result.success)
                {
                    // ShowError(result.error);
                }
            }
            else
            {
                // Actualizar existente
                TeamService.UpdatePokemon(_editingPosition, pokemon);
            }


            _showPokemonEditorWindow = false;

            _editedBasePokemon = null;
            _editingPosition = 0; // 0 = nuevo, 1-6 = editar existente

            // Pokemon Data
            _editedPokemonName = "";
            _editedNickname = "";
            _editedLevel = 50;
            _editedGenderIndex = 0; // 0 - Male | 1 - Female | 2 - Genderless
            _editedNatureIndex = 0;
            _editedAbilityIndex = 0;
            _editedItemIndex = 0;

            // Stats
            _editedIVsHP = 31;
            _editedIVsATTACK = 31;
            _editedIVsDEFENSE = 31;
            _editedIVsSPATTACK = 31;
            _editedIVsSPDEFENSE = 31;
            _editedIVsSPEED = 31;

            _editedEVsHP = 0;
            _editedEVsATTACK = 0;
            _editedEVsDEFENSE = 0;
            _editedEVsSPATTACK = 0;
            _editedEVsSPDEFENSE = 0;
            _editedEVsSPEED = 0;


            _editedIvs = new(31, 31, 31, 31, 31, 31);
            _editedEvs = new PokemonStats();
            _editedCalculatedStats = new PokemonStats();

            // Moveset
            _selectedMoves = new();
        }

        /// <summary>
        /// Generates 3 seed teams for UI testing (1, 3, and 6 Pokemon teams)
        /// </summary>
        private static async Task GenerateSeedTeams()
        {
            try
            {
                Console.WriteLine("Generating seed teams...");
                // Team 1: Single Pokemon (Charizard)
                {
                    TeamService.ClearTeam();
                    var charizard = await PokemonService.GetPokemon("charizard");
                    if (charizard != null)
                    {
                        var playerCharizard = new PlayerPokemon(charizard, "Blaze", 50)
                        {
                            Nickname = "Flamey",
                            Gender = PokemonGender.Male,
                            Nature = Nature.GetNature("Timid") ?? Nature.GetAllNatures()[0],
                            Ability = new Ability("Blaze"),
                            HeldItem = Item.GetItem("Life Orb"),
                            IVs = new PokemonStats(31, 0, 31, 31, 31, 31),
                            EVs = new PokemonStats(4, 0, 0, 252, 0, 252),
                            CurrentHP = 153,
                            BaseStats = charizard.BaseStats,
                            Moveset = new List<LearnedMove>
                            {
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Flamethrower", Power = 90, Accuracy = 100, PP = 15,
                                        DamageClass = MoveDamageClass.Special
                                    }, 15),
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Air Slash", Power = 75, Accuracy = 95, PP = 15,
                                        DamageClass = MoveDamageClass.Special
                                    }, 15),
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Dragon Pulse", Power = 85, Accuracy = 100, PP = 10,
                                        DamageClass = MoveDamageClass.Special
                                    }, 10),
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Focus Blast", Power = 120, Accuracy = 70, PP = 5,
                                        DamageClass = MoveDamageClass.Special
                                    }, 5)
                            }
                        };
                        TeamService.AddPokemon(playerCharizard);
                        ConfigService.SaveTeam("Test Team 1");
                        Console.WriteLine("✓ Created Test Team 1 (1 Pokemon)");
                    }
                }
                // Team 2: Three Pokemon (Blastoise, Venusaur, Pikachu)
                {
                    TeamService.ClearTeam();

                    var blastoise = await PokemonService.GetPokemon("blastoise");
                    if (blastoise != null)
                    {
                        var playerBlastoise = new PlayerPokemon(blastoise, "Torrent", 50)
                        {
                            Nickname = "Shellshock",
                            Gender = PokemonGender.Male,
                            Nature = Nature.GetNature("Modest") ?? Nature.GetAllNatures()[0],
                            Ability = new Ability("Torrent"),
                            HeldItem = Item.GetItem("Leftovers"),
                            IVs = new PokemonStats(31, 0, 31, 31, 31, 31),
                            EVs = new PokemonStats(252, 0, 0, 252, 4, 0),
                            BaseStats = blastoise.BaseStats,
                            Moveset = new List<LearnedMove>
                            {
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Surf", Power = 90, Accuracy = 100, PP = 15,
                                        DamageClass = MoveDamageClass.Special
                                    }, 15),
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Ice Beam", Power = 90, Accuracy = 100, PP = 10,
                                        DamageClass = MoveDamageClass.Special
                                    }, 10)
                            }
                        };
                        TeamService.AddPokemon(playerBlastoise);
                    }

                    var venusaur = await PokemonService.GetPokemon("venusaur");
                    if (venusaur != null)
                    {
                        var playerVenusaur = new PlayerPokemon(venusaur, "Overgrow", 50)
                        {
                            Gender = PokemonGender.Female,
                            Nature = Nature.GetNature("Calm") ?? Nature.GetAllNatures()[0],
                            Ability = new Ability("Overgrow"),
                            IVs = new PokemonStats(31, 0, 31, 31, 31, 31),
                            EVs = new PokemonStats(252, 0, 4, 0, 252, 0),
                            BaseStats = venusaur.BaseStats,
                            Moveset = new List<LearnedMove>
                            {
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Giga Drain", Power = 75, Accuracy = 100, PP = 10,
                                        DamageClass = MoveDamageClass.Special
                                    }, 10),
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Sludge Bomb", Power = 90, Accuracy = 100, PP = 10,
                                        DamageClass = MoveDamageClass.Special
                                    }, 10)
                            }
                        };
                        TeamService.AddPokemon(playerVenusaur);
                    }

                    var pikachu = await PokemonService.GetPokemon("pikachu");
                    if (pikachu != null)
                    {
                        var playerPikachu = new PlayerPokemon(pikachu, "Static", 50)
                        {
                            Nickname = "Sparky",
                            Gender = PokemonGender.Male,
                            Nature = Nature.GetNature("Jolly") ?? Nature.GetAllNatures()[0],
                            Ability = new Ability("Static"),
                            HeldItem = Item.GetItem("Choice Band"),
                            IVs = new PokemonStats(31, 31, 31, 0, 31, 31),
                            EVs = new PokemonStats(4, 252, 0, 0, 0, 252),
                            BaseStats = pikachu.BaseStats,
                            Moveset = new List<LearnedMove>
                            {
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Thunder", Power = 110, Accuracy = 70, PP = 10,
                                        DamageClass = MoveDamageClass.Special
                                    }, 10),
                                new LearnedMove(
                                    new PokemonMove
                                    {
                                        Name = "Quick Attack", Power = 40, Accuracy = 100, PP = 30,
                                        DamageClass = MoveDamageClass.Physical
                                    }, 30)
                            }
                        };
                        TeamService.AddPokemon(playerPikachu);
                    }

                    ConfigService.SaveTeam("Test Team 3");
                    Console.WriteLine("✓ Created Test Team 3 (3 Pokemon)");
                }
                // Team 3: Full team (6 Pokemon)
                {
                    TeamService.ClearTeam();

                    string[] pokemonNames = { "gyarados", "alakazam", "gengar", "dragonite", "snorlax", "machamp" };

                    foreach (var name in pokemonNames)
                    {
                        var pokemon = await PokemonService.GetPokemon(name);
                        if (pokemon != null)
                        {
                            var playerPokemon = new PlayerPokemon(pokemon, pokemon.Name, 50)
                            {
                                Gender = PokemonGender.Male,
                                Nature = Nature.GetAllNatures()[0],
                                Ability = new Ability(pokemon.Name),
                                IVs = new PokemonStats(31, 31, 31, 31, 31, 31),
                                EVs = new PokemonStats(252, 0, 0, 252, 0, 4),
                                BaseStats = pokemon.BaseStats,
                                Moveset = new List<LearnedMove>
                                {
                                    new LearnedMove(
                                        new PokemonMove
                                        {
                                            Name = "Tackle", Power = 40, Accuracy = 100, PP = 35,
                                            DamageClass = MoveDamageClass.Physical
                                        }, 35)
                                }
                            };
                            TeamService.AddPokemon(playerPokemon);
                        }
                    }

                    ConfigService.SaveTeam("Test Team 6");
                    Console.WriteLine("✓ Created Test Team 6 (6 Pokemon)");
                }
                Console.WriteLine("✅ All seed teams generated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating seed teams: {ex.Message}");
            }
        }


        public RenderPasaporte() : base("PasaporteFiller")
        {
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += ScreenTextRecognizer.CheckForPokemonName;
            timer.Start();

            ConfigService.Initialize();
            var teams = ConfigService.GetSavedTeams();
            if (teams.Count > 0)
            {
                ConfigService.LoadTeam(teams[0]);
                _teamComboPreview = teams[0];
                _currentTeamName = teams[0];
            }
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


            if (_checkForPokemonInScreenEnabled)
            {
                if (ScreenTextRecognizer.LastRecognizedPokemonName != PokemonNameSearch)
                {
                    PokemonNameSearch = ScreenTextRecognizer.LastRecognizedPokemonName;
                    SearchPokemon();
                }
            }

            ImGui.Begin("Busqueda", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize);


            ImGui.Checkbox("Show Team Manager", ref _showTeamManagerWindow);
            ImGui.Spacing();
            ImGui.Checkbox("Mostrar Configuración", ref _showConfigurationWindow);
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
            if (ImGui.IsKeyReleased(ImGuiKey.Enter)) SearchPokemon();

            ImGui.End();
            ImGui.Begin("Información Pokémon", ref _isOpenInfoWindow,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize);
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
                    ImGui.Image(Type1ImageHandle, new Vector2(Type1ImageWidth / 4, Type1ImageHeight / 4));
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

            // Team Manager Window

            // Recuerda onLoad Team hacer: _currentTeamName = TeamService.CurrentTeamName 
            if (_showTeamManagerWindow)
            {
                ImGui.Begin("Team Manager",
                    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Modal);
                // Team View

                var flags1 = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PadOuterX;

                if (ImGui.BeginTable("##team_manager_list", 3, flags1))
                {
                    ImGui.TableSetupColumn("Pokemon", ImGuiTableColumnFlags.WidthFixed, 215);
                    ImGui.TableSetupColumn("Edit", ImGuiTableColumnFlags.WidthFixed, 45);
                    ImGui.TableSetupColumn("Del", ImGuiTableColumnFlags.WidthFixed, 45);

                    for (int i = 0; i < 6; i++)
                    {
                        var pokemonInSlot = TeamService.GetPokemon(i + 1);
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        var textToShow = pokemonInSlot == null
                            ? $"{i + 1}. [Empty Slot]"
                            : $"{i + 1}. [{pokemonInSlot.BaseData.Name} ({pokemonInSlot.Nickname}) Lv.{pokemonInSlot.Level}]";
                        ImGui.TextUnformatted(textToShow);

                        ImGui.TableSetColumnIndex(1);
                        if (pokemonInSlot == null)
                        {
                            ImGui.Button("Add");
                        }
                        else
                        {
                            if (ImGui.Button("Edit"))
                            {
                                _showPokemonEditorWindow = true;
                                _editedBasePokemon = pokemonInSlot.BaseData;

                                _editingPosition = i;

                                _editedPokemonName = pokemonInSlot.BaseData.Name;
                                _editedNickname = pokemonInSlot.Nickname;
                                _editedLevel = pokemonInSlot.Level;
                                _editedGenderIndex = (int)pokemonInSlot.Gender;
                                _editedNatureIndex = 0; // Nature.GetAllNatures().IndexOf(pokemonInSlot.Nature)
                                _editedAbilityIndex = 0; // Ability.GetCommonAbilities().IndexOf(pokemonInSlot.Ability)
                                _editedItemIndex = 0; // Item.GetCommonItems().IndexOf(pokemonInSlot.HeldItem)

                                _editedIVsHP = pokemonInSlot.IVs.HP;
                                _editedIVsATTACK = pokemonInSlot.IVs.Attack;
                                _editedIVsDEFENSE = pokemonInSlot.IVs.Defense;
                                _editedIVsSPATTACK = pokemonInSlot.IVs.SpecialAttack;
                                _editedIVsSPDEFENSE = pokemonInSlot.IVs.SpecialDefense;
                                _editedIVsSPEED = pokemonInSlot.IVs.Speed;

                                _editedEVsHP = pokemonInSlot.EVs.HP;
                                _editedEVsATTACK = pokemonInSlot.EVs.Attack;
                                _editedEVsDEFENSE = pokemonInSlot.EVs.Defense;
                                _editedEVsSPATTACK = pokemonInSlot.EVs.SpecialAttack;
                                _editedEVsSPDEFENSE = pokemonInSlot.EVs.SpecialDefense;
                                _editedEVsSPEED = pokemonInSlot.EVs.Speed;

                                _editedIvs = pokemonInSlot.IVs;
                                _editedEvs = pokemonInSlot.EVs;
                                _editedCalculatedStats = pokemonInSlot.CalculatedStats;
                                _selectedMoves = pokemonInSlot.Moveset;
                            }
                        }

                        ImGui.TableSetColumnIndex(2);
                        if (pokemonInSlot != null)
                        {
                            if (ImGui.Button("Del"))
                            {
                                TeamService.RemovePokemon(i + 1);
                            }
                        }
                    }

                    ImGui.EndTable();
                }


                ImGui.Separator();
                // Selector 
                var flags2 = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PadOuterX;

                if (ImGui.BeginTable("##team_manager_selector", 3, flags2))
                {
                    ImGui.TableSetupColumn("Left", ImGuiTableColumnFlags.WidthFixed, 75);
                    ImGui.TableSetupColumn("Middle", ImGuiTableColumnFlags.WidthFixed, 180);
                    ImGui.TableSetupColumn("Right", ImGuiTableColumnFlags.WidthFixed, 45);


                    // Topside
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted("Team Name:");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.SetNextItemWidth(-1);
                    ImGui.InputText("##CurrentTeamNameField", ref _currentTeamName, 64);

                    ImGui.TableSetColumnIndex(2);
                    // Save Team

                    if (ImGui.Button("Save"))
                    {
                        var temporalCurrentTeamName = _currentTeamName;
                        if (temporalCurrentTeamName.Length > 0)
                        {
                            var teamsBeforeSave = ConfigService.GetSavedTeams();
                            if (teamsBeforeSave.Contains(temporalCurrentTeamName))
                            {
                                _askConfirmReplace = true;
                                _confirmed = _cancelled = false;
                                ImGui.OpenPopup("ConfirmReplaceTeamFile");
                            }
                            else
                            {
                                _confirmed = true;
                            }
                        }
                    }

                    if (_askConfirmReplace)
                    {
                        ImGui.SetNextWindowSize(new Vector2(420, 0), ImGuiCond.Appearing);

                        bool open = true;

                        if (ImGui.BeginPopupModal("ConfirmReplaceTeamFile", ref open,
                                ImGuiWindowFlags.AlwaysAutoResize))
                        {
                            ImGui.TextWrapped(
                                $"Do you want to replace the existing file '{_currentTeamName}.json'?, this action cannot be undone.");
                            ImGui.Spacing();
                            if (ImGui.Button("Continue", new Vector2(146, 0)))
                            {
                                _confirmed = true;
                                _askConfirmReplace = false;
                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.SameLine();

                            if (ImGui.Button("Cancel", new Vector2(140, 0)))
                            {
                                _cancelled = true;
                                _askConfirmReplace = false;
                                ImGui.CloseCurrentPopup();
                            }

                            if (!open)
                            {
                                _cancelled = true;
                                _askConfirmReplace = false;
                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.EndPopup();
                        }
                    }

                    if (_confirmed)
                    {
                        ConfigService.SaveTeam(_currentTeamName);
                        var teams = ConfigService.GetSavedTeams();

                        if (teams.Count > 0)
                        {
                            ConfigService.LoadTeam(_currentTeamName);
                        }
                    }

                    _confirmed = false;

                    // Downside
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted("Load Team:");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.BeginCombo("##LoadTeamCombo", _teamComboPreview))
                    {
                        ConfigService.LoadSettings();
                        foreach (var team in ConfigService.GetSavedTeams())
                        {
                            bool isSelected = _teamComboPreview == team;
                            if (ImGui.Selectable(team, isSelected))
                                _teamComboPreview = team;

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.TableSetColumnIndex(2);

                    // Load Team

                    if (ImGui.Button("Load"))
                    {
                        if (_teamComboPreview.Length > 0)
                        {
                            if (ConfigService.LoadTeam(_teamComboPreview))
                            {
                                _currentTeamName = _teamComboPreview;
                            }
                        }
                    }

                    ImGui.EndTable();
                }


                if (_showPokemonEditorWindow)
                {
                    _editedIvs = new PokemonStats(
                        _editedIVsHP,
                        _editedIVsATTACK,
                        _editedIVsDEFENSE,
                        _editedIVsSPATTACK,
                        _editedIVsSPDEFENSE,
                        _editedIVsSPEED
                    );

                    if (_editedBasePokemon != null)
                    {
                        // IVs Section
                        ImGui.Begin("Edit Pokemon");
                        if (ImGui.TreeNode("IVs (0-31)"))
                        {
                            ImGui.SliderInt("HP IV", ref _editedIVsHP, 0, 31);
                            ImGui.SliderInt("Attack IV", ref _editedIVsATTACK, 0, 31);
                            ImGui.SliderInt("Defense IV", ref _editedIVsDEFENSE, 0, 31);
                            ImGui.SliderInt("Sp.Atk IV", ref _editedIVsSPATTACK, 0, 31);
                            ImGui.SliderInt("Sp.Def IV", ref _editedIVsSPDEFENSE, 0, 31);
                            ImGui.SliderInt("Speed IV", ref _editedIVsSPEED, 0, 31);
                            if (ImGui.Button("Max All IVs"))
                            {
                                _editedIVsHP = 31;
                                _editedIVsATTACK = 31;
                                _editedIVsDEFENSE = 31;
                                _editedIVsSPATTACK = 31;
                                _editedIVsSPDEFENSE = 31;
                                _editedIVsSPEED = 31;
                            }

                            ImGui.TreePop();
                        }

                        // EVs Section
                        if (ImGui.TreeNode("EVs (0-252, Total 510)"))
                        {
                            int totalEVs = StatsCalculator.GetTotalEVs(_editedEvs);
                            ImGui.Text($"Total EVs: {totalEVs}/510");

                            ImGui.SliderInt("HP EV", ref _editedEVsHP, 0, 252);
                            ImGui.SliderInt("Attack EV", ref _editedEVsATTACK, 0, 252);
                            ImGui.SliderInt("Defense EV", ref _editedEVsDEFENSE, 0, 252);
                            ImGui.SliderInt("Sp.Atk EV", ref _editedEVsSPATTACK, 0, 252);
                            ImGui.SliderInt("Sp.Def EV", ref _editedEVsSPDEFENSE, 0, 252);
                            ImGui.SliderInt("Speed EV", ref _editedEVsSPEED, 0, 252);

                            var evValidation = StatsCalculator.ValidateEVs(_editedEvs);
                            if (!evValidation.isValid)
                            {
                                ImGui.TextColored(new Vector4(1, 0, 0, 1), evValidation.error);
                            }

                            ImGui.TreePop();
                        }

                        CalculateStatsRealTime();

                        if (ImGui.TreeNode("Calculated Stats"))
                        {
                            ImGui.Text($"HP: {_editedCalculatedStats.HP}");
                            ImGui.Text($"Attack: {_editedCalculatedStats.Attack}");
                            ImGui.Text($"Defense: {_editedCalculatedStats.Defense}");
                            ImGui.Text($"Sp.Atk: {_editedCalculatedStats.SpecialAttack}");
                            ImGui.Text($"Sp.Def: {_editedCalculatedStats.SpecialDefense}");
                            ImGui.Text($"Speed: {_editedCalculatedStats.Speed}");
                            ImGui.TreePop();
                        }

                        if (ImGui.Button("Save"))
                        {
                            SavePokemonEditor();
                        }

                        ImGui.End();
                    }
                }

                ImGui.End();
            }


            // Configuration Window
            if (_showConfigurationWindow)
            {
                ImGui.Begin("Configuración",
                    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Modal);
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


                    ImGui.BeginTable("##Positions", 4, ImGuiTableFlags.Borders);
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
                        ImGui.Begin("Area de visión",
                            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
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

                            // if (_secondPointY < _firstPointY)
                            //     (_firstPointY, _secondPointY) = (_secondPointY, _firstPointY);
                            // if (_secondPointX < _firstPointX)
                            //     (_firstPointX, _secondPointX) = (_secondPointX, _firstPointX);


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
