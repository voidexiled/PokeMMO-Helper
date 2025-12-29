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

        /// <summary>
        /// Formats stat names for display in tooltips
        /// </summary>
        private static string FormatStatName(string statName)
        {
            if (string.IsNullOrEmpty(statName))
                return "";

            return statName.ToLower() switch
            {
                "hp" => "HP",
                "attack" => "Attack",
                "defense" => "Defense",
                "specialattack" => "Sp. Attack",
                "specialdefense" => "Sp. Defense",
                "speed" => "Speed",
                _ => statName
            };
        }

        /// <summary>
        /// Renders a single type icon for move tooltips
        /// </summary>
        private void RenderMoveTypeIcon(string typeName, Vector2 size)
        {
            if (string.IsNullOrEmpty(typeName)) return;

            typeName = typeName.ToLower();
            IntPtr handle = typeName switch
            {
                "bug" => BugTypeImageHandle,
                "dark" => DarkTypeImageHandle,
                "dragon" => DragonTypeImageHandle,
                "electric" => ElectricTypeImageHandle,
                "fairy" => FairyTypeImageHandle,
                "fighting" => FightingTypeImageHandle,
                "fire" => FireTypeImageHandle,
                "flying" => FlyingTypeImageHandle,
                "ghost" => GhostTypeImageHandle,
                "grass" => GrassTypeImageHandle,
                "ground" => GroundTypeImageHandle,
                "ice" => IceTypeImageHandle,
                "normal" => NormalTypeImageHandle,
                "poison" => PoisonTypeImageHandle,
                "psychic" => PsychicTypeImageHandle,
                "rock" => RockTypeImageHandle,
                "steel" => SteelTypeImageHandle,
                "water" => WaterTypeImageHandle,
                _ => IntPtr.Zero
            };

            if (handle != IntPtr.Zero)
            {
                ImGui.Image(handle, size);
            }
        }

        /// <summary>
        /// Formats damage class for display
        /// </summary>
        private static string FormatDamageClass(MoveDamageClass damageClass)
        {
            return damageClass switch
            {
                MoveDamageClass.Physical => "Physical",
                MoveDamageClass.Special => "Special",
                MoveDamageClass.Status => "Status",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Returns color for move power based on strength
        /// </summary>
        private Vector4 GetPowerColor(int power)
        {
            return power switch
            {
                >= 100 => new Vector4(0.2f, 1.0f, 0.2f, 1.0f),  // Green - Very strong
                >= 60 => new Vector4(1.0f, 1.0f, 0.2f, 1.0f),   // Yellow - Decent
                > 0 => new Vector4(0.4f, 0.8f, 1.0f, 1.0f),     // Blue - Weak
                _ => new Vector4(0.7f, 0.7f, 0.7f, 1.0f)        // Gray - Status move
            };
        }

        /// <summary>
        /// Renders a colored badge for damage class
        /// </summary>
        private void RenderDamageClassBadge(MoveDamageClass damageClass)
        {
            var (text, color) = damageClass switch
            {
                MoveDamageClass.Physical => ("PHYSICAL", new Vector4(0.9f, 0.5f, 0.2f, 1.0f)),  // Orange
                MoveDamageClass.Special => ("SPECIAL", new Vector4(0.6f, 0.35f, 0.7f, 1.0f)),   // Purple
                MoveDamageClass.Status => ("STATUS", new Vector4(0.2f, 0.6f, 0.9f, 1.0f)),      // Blue
                _ => ("UNKNOWN", new Vector4(0.5f, 0.5f, 0.5f, 1.0f))
            };

            // Center the badge
            var textSize = ImGui.CalcTextSize(text);
            ImGui.Indent((320 - textSize.X) / 2); // Center in 320px tooltip
            ImGui.PushStyleColor(ImGuiCol.Button, color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color * 1.2f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, color * 0.8f);
            ImGui.SmallButton(text);
            ImGui.PopStyleColor(3);
            ImGui.Unindent((320 - textSize.X) / 2);
        }

        /// <summary>
        /// Formats move target with icon
        /// </summary>
        private static string FormatMoveTarget(MoveTarget target)
        {
            return target switch
            {
                MoveTarget.SelectedPokemon => "Target: Single Pokemon",
                MoveTarget.AllOpponents => "Target: All Opponents",
                MoveTarget.AllOtherPokemon => "Target: All Other Pokemon",
                MoveTarget.User => "Target: Self",
                MoveTarget.UserAndAllies => "Target: User & Allies",
                MoveTarget.AllPokemon => "Target: All Pokemon",
                MoveTarget.RandomOpponent => "Target: Random Opponent",
                MoveTarget.AllAllies => "Target: All Allies",
                MoveTarget.UserOrAlly => "Target: User or Ally",
                MoveTarget.OpponentsField => "Target: Opponent's Field",
                MoveTarget.UsersField => "Target: User's Field",
                MoveTarget.EntireField => "Target: Entire Field",
                _ => "Target: Unknown"
            };
        }

        /// <summary>
        /// Returns color for Pokemon type
        /// </summary>
        private Vector4 GetTypeColor(string typeName)
        {
            return typeName.ToLower() switch
            {
                "fire" => new Vector4(0.93f, 0.51f, 0.19f, 1.0f),       // Orange-red
                "water" => new Vector4(0.39f, 0.56f, 0.93f, 1.0f),      // Blue
                "grass" => new Vector4(0.48f, 0.78f, 0.30f, 1.0f),      // Green
                "electric" => new Vector4(0.98f, 0.84f, 0.25f, 1.0f),   // Yellow
                "normal" => new Vector4(0.66f, 0.66f, 0.47f, 1.0f),     // Tan
                "fighting" => new Vector4(0.75f, 0.19f, 0.15f, 1.0f),   // Dark red
                "flying" => new Vector4(0.66f, 0.71f, 0.95f, 1.0f),     // Light blue
                "poison" => new Vector4(0.64f, 0.25f, 0.65f, 1.0f),     // Purple
                "ground" => new Vector4(0.89f, 0.75f, 0.42f, 1.0f),     // Sandy
                "rock" => new Vector4(0.72f, 0.64f, 0.42f, 1.0f),       // Brown
                "bug" => new Vector4(0.65f, 0.75f, 0.19f, 1.0f),        // Yellow-green
                "ghost" => new Vector4(0.44f, 0.35f, 0.60f, 1.0f),      // Dark purple
                "steel" => new Vector4(0.72f, 0.72f, 0.82f, 1.0f),      // Silver
                "psychic" => new Vector4(0.98f, 0.41f, 0.55f, 1.0f),    // Pink
                "ice" => new Vector4(0.60f, 0.85f, 0.85f, 1.0f),        // Cyan
                "dragon" => new Vector4(0.44f, 0.22f, 0.98f, 1.0f),     // Purple-blue
                "dark" => new Vector4(0.44f, 0.35f, 0.30f, 1.0f),       // Dark brown
                "fairy" => new Vector4(0.85f, 0.52f, 0.73f, 1.0f),      // Pink
                _ => new Vector4(1.0f, 1.0f, 1.0f, 1.0f)                // White
            };
        }

        /// <summary>
        /// Calculates move power with STAB (Same Type Attack Bonus) if applicable
        /// Returns null if no STAB bonus
        /// </summary>
        private int? CalculateSTABPower(PokemonMove move, Pokemon pokemon)
        {
            if (string.IsNullOrEmpty(move.TypeName) || pokemon?.Types == null || pokemon.Types.Count == 0)
                return null;

            // Check if move type matches any of Pokemon's types
            bool hasSTAB = pokemon.Types.Any(t =>
                t.Name.Equals(move.TypeName, StringComparison.OrdinalIgnoreCase));

            if (hasSTAB && move.Power > 0)
            {
                return (int)(move.Power * 1.5f); // STAB is 1.5x damage
            }

            return null;
        }

        /// <summary>
        /// Gets type effectiveness for a move type using cached Pokemon type data
        /// Returns lists of super effective and not very effective types
        /// </summary>
        private (List<string> superEffective, List<string> notEffective) GetMoveEffectiveness(string moveType)
        {
            var superEffective = new List<string>();
            var notEffective = new List<string>();

            if (string.IsNullOrEmpty(moveType))
                return (superEffective, notEffective);

            try
            {
                // Get type effectiveness from Pokemon API
                // Index 3 = double damage to, 4 = half damage to, 5 = no damage to
                var effectiveness = PokemonService.GetTypeWeaknessesAndStrengths(moveType).GetAwaiter().GetResult();

                if (effectiveness != null && effectiveness.Count >= 6)
                {
                    superEffective = effectiveness[3] ?? new List<string>();
                    notEffective = (effectiveness[4] ?? new List<string>())
                        .Concat(effectiveness[5] ?? new List<string>())
                        .ToList();
                }
            }
            catch
            {
                // If API call fails, return empty lists
            }

            return (superEffective, notEffective);
        }

        /// <summary>
        /// Renders a compact type effectiveness preview
        /// </summary>
        private void RenderEffectivenessPreview(string moveType, float maxWidth)
        {
            var (superEffective, notEffective) = GetMoveEffectiveness(moveType);

            if (superEffective.Count > 0)
            {
                ImGui.TextColored(new Vector4(0.3f, 1.0f, 0.3f, 1.0f), "Super effective:");
                ImGui.SameLine();

                // Render type icons in a compact row
                foreach (var type in superEffective.Take(6)) // Limit to 6 for space
                {
                    RenderMoveTypeIcon(type, new Vector2(16, 16));
                    ImGui.SameLine();
                }
                ImGui.NewLine();
            }

            if (notEffective.Count > 0)
            {
                ImGui.TextColored(new Vector4(1.0f, 0.5f, 0.3f, 1.0f), "Not very effective:");
                ImGui.SameLine();

                // Render type icons in a compact row
                foreach (var type in notEffective.Take(6)) // Limit to 6 for space
                {
                    RenderMoveTypeIcon(type, new Vector2(16, 16));
                    ImGui.SameLine();
                }
                ImGui.NewLine();
            }
        }

        /// <summary>
        /// Renders Pokemon type badges and effectiveness information
        /// </summary>
        private void RenderPokemonTypeInfo(Pokemon pokemon)
        {
            if (pokemon?.Types == null || pokemon.Types.Count == 0)
                return;

            ImGui.BeginGroup();

            // Header
            ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1.0f), "Type & Effectiveness");
            ImGui.Separator();
            ImGui.Spacing();

            // Pokemon Types with icons
            ImGui.Text("Types:");
            ImGui.SameLine();
            foreach (var type in pokemon.Types)
            {
                RenderMoveTypeIcon(type.Name, new Vector2(32, 32));

                // Tooltip with type name
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextColored(GetTypeColor(type.Name), char.ToUpper(type.Name[0]) + type.Name.Substring(1));
                    ImGui.EndTooltip();
                }

                ImGui.SameLine();
            }
            ImGui.NewLine();

            ImGui.Spacing();

            // Calculate combined effectiveness
            var effectiveness = CalculateCombinedEffectiveness(pokemon);

            // Weaknesses (damage taken)
            var weaknesses4x = effectiveness.Where(e => e.Value >= 3.9f).ToList();
            var weaknesses2x = effectiveness.Where(e => e.Value >= 1.9f && e.Value < 3.9f).ToList();

            if (weaknesses4x.Any() || weaknesses2x.Any())
            {
                ImGui.TextColored(new Vector4(1.0f, 0.4f, 0.4f, 1.0f), "Weak to:");

                if (weaknesses4x.Any())
                {
                    ImGui.Text("  4x:");
                    ImGui.SameLine();
                    foreach (var w in weaknesses4x)
                    {
                        RenderMoveTypeIcon(w.Key, new Vector2(32, 32));
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextColored(GetTypeColor(w.Key), w.Key);
                            ImGui.EndTooltip();
                        }
                        ImGui.SameLine();
                    }
                    ImGui.NewLine();
                }

                if (weaknesses2x.Any())
                {
                    ImGui.Text("  2x:");
                    ImGui.SameLine();
                    foreach (var w in weaknesses2x)
                    {
                        RenderMoveTypeIcon(w.Key, new Vector2(32, 32));
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextColored(GetTypeColor(w.Key), w.Key);
                            ImGui.EndTooltip();
                        }
                        ImGui.SameLine();
                    }
                    ImGui.NewLine();
                }

                ImGui.Spacing();
            }

            // Resistances (reduced damage)
            var resistances14x = effectiveness.Where(e => e.Value <= 0.26f && e.Value > 0).ToList();
            var resistances12x = effectiveness.Where(e => e.Value <= 0.51f && e.Value > 0.26f).ToList();

            if (resistances14x.Any() || resistances12x.Any())
            {
                ImGui.TextColored(new Vector4(0.4f, 1.0f, 0.4f, 1.0f), "Resistant to:");

                if (resistances14x.Any())
                {
                    ImGui.Text("  1/4x:");
                    ImGui.SameLine();
                    foreach (var r in resistances14x)
                    {
                        RenderMoveTypeIcon(r.Key, new Vector2(32, 32));
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextColored(GetTypeColor(r.Key), r.Key);
                            ImGui.EndTooltip();
                        }
                        ImGui.SameLine();
                    }
                    ImGui.NewLine();
                }

                if (resistances12x.Any())
                {
                    ImGui.Text("  1/2x:");
                    ImGui.SameLine();
                    foreach (var r in resistances12x)
                    {
                        RenderMoveTypeIcon(r.Key, new Vector2(32, 32));
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextColored(GetTypeColor(r.Key), r.Key);
                            ImGui.EndTooltip();
                        }
                        ImGui.SameLine();
                    }
                    ImGui.NewLine();
                }

                ImGui.Spacing();
            }

            // Immunities
            var immunities = effectiveness.Where(e => e.Value == 0f).ToList();
            if (immunities.Any())
            {
                ImGui.TextColored(new Vector4(0.6f, 0.6f, 1.0f, 1.0f), "Immune to:");
                ImGui.SameLine();
                foreach (var imm in immunities)
                {
                    RenderMoveTypeIcon(imm.Key, new Vector2(32, 32));
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.TextColored(GetTypeColor(imm.Key), imm.Key);
                        ImGui.EndTooltip();
                    }
                    ImGui.SameLine();
                }
                ImGui.NewLine();
            }

            ImGui.EndGroup();
        }

        /// <summary>
        /// Calculates combined type effectiveness for all types
        /// </summary>
        private Dictionary<string, float> CalculateCombinedEffectiveness(Pokemon pokemon)
        {
            var combined = new Dictionary<string, float>();

            if (pokemon?.Types == null || pokemon.Types.Count == 0)
                return combined;

            // Get all possible attacking types
            var allTypes = new[] { "normal", "fire", "water", "electric", "grass", "ice", "fighting",
                "poison", "ground", "flying", "psychic", "bug", "rock", "ghost", "dragon",
                "dark", "steel", "fairy" };

            foreach (var attackType in allTypes)
            {
                float multiplier = 1.0f;

                // Multiply effectiveness from each of Pokemon's types
                foreach (var defenseType in pokemon.Types)
                {
                    var typeEffectiveness = GetTypeMultiplier(attackType, defenseType);
                    multiplier *= typeEffectiveness;
                }

                combined[char.ToUpper(attackType[0]) + attackType.Substring(1)] = multiplier;
            }

            return combined;
        }

        /// <summary>
        /// Gets type multiplier for attack type vs defense type
        /// </summary>
        private float GetTypeMultiplier(string attackType, PokemonType defenseType)
        {
            if (defenseType.NoDamageFrom?.Any(t => t.Equals(attackType, StringComparison.OrdinalIgnoreCase)) == true)
                return 0f;

            if (defenseType.DoubleDamageFrom?.Any(t => t.Equals(attackType, StringComparison.OrdinalIgnoreCase)) == true)
                return 2f;

            if (defenseType.HalfDamageFrom?.Any(t => t.Equals(attackType, StringComparison.OrdinalIgnoreCase)) == true)
                return 0.5f;

            return 1f;
        }


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
        private string _pokemonSearchError = "";
        private string _moveSearchFilter = "";
        private string _natureSearchFilter = "";
        private string _abilitySearchFilter = "";
        private string _itemSearchFilter = "";
        private string[] _moveSlotSearchFilters = new string[4] { "", "", "", "" };
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

        // Loading screen state
        private bool _isLoadingData = false;
        private bool _dataLoaded = false;
        private Task? _loadingTask = null;

        // Background loading state
        private LoadingProgress? _backgroundProgress = null;
        private Task? _backgroundLoadingTask = null;



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

        // Moveset search filter fields (persist between frames)
        private string[] _moveSearchFilters = new string[] { "", "", "", "" };

        /// <summary>
        /// Renders loading screen during startup data loading
        /// </summary>
        private void RenderLoadingScreen()
        {
            ImGui.SetNextWindowSize(new Vector2(500, 220));
            ImGui.SetNextWindowPos(new Vector2(
                ImGui.GetIO().DisplaySize.X / 2 - 250,
                ImGui.GetIO().DisplaySize.Y / 2 - 110
            ));

            ImGui.Begin("Loading", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);

            ImGui.Spacing();
            ImGui.Spacing();

            // Title
            ImGui.SetWindowFontScale(1.5f);
            var textWidth = ImGui.CalcTextSize("PasaporteFiller").X;
            ImGui.SetCursorPosX((500 - textWidth) / 2);
            ImGui.TextColored(new Vector4(0.4f, 0.8f, 1.0f, 1.0f), "PasaporteFiller");
            ImGui.SetWindowFontScale(1.0f);

            ImGui.Spacing();
            ImGui.Spacing();

            // Status
            var statusWidth = ImGui.CalcTextSize(DataLoader.LoadingStatus).X;
            ImGui.SetCursorPosX((500 - statusWidth) / 2);
            ImGui.Text(DataLoader.LoadingStatus);

            ImGui.Spacing();

            // Progress bar
            ImGui.ProgressBar(DataLoader.LoadingProgress, new Vector2(-1, 30));

            ImGui.Spacing();

            // Error message and buttons
            if (!string.IsNullOrEmpty(DataLoader.ErrorMessage))
            {
                ImGui.PushTextWrapPos(480);
                ImGui.TextColored(new Vector4(1, 0.3f, 0.3f, 1.0f), $"Error: {DataLoader.ErrorMessage}");
                ImGui.PopTextWrapPos();

                ImGui.Spacing();

                // Retry button
                if (ImGui.Button("Retry", new Vector2(100, 30)))
                {
                    DataLoader.Reset();
                    _isLoadingData = false;
                    _dataLoaded = false;
                    _loadingTask = null;
                }

                ImGui.SameLine();

                // Skip button
                if (ImGui.Button("Skip (Use Offline Data)", new Vector2(200, 30)))
                {
                    _dataLoaded = true;
                    _isLoadingData = false;
                }
            }

            ImGui.End();
        }

        protected override void Render()
        {
            // Show loading screen if data not loaded
            if (!_dataLoaded && !_isLoadingData)
            {
                // Only start loading once - check if task is null or completed (not running)
                if (_loadingTask == null || _loadingTask.IsCompleted)
                {
                    _isLoadingData = true;
                    Console.WriteLine("[RENDER] Starting minimal data load...");
                    _loadingTask = Task.Run(async () =>
                    {
                        try
                        {
                            await DataLoader.LoadMinimalData();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[RENDER] DataLoader failed: {ex.Message}");
                            // DataLoader handles its own ErrorMessage internally
                        }
                    });
                }
            }

            if (!_dataLoaded)
            {
                RenderLoadingScreen();

                // Check if loading completed
                if (_loadingTask != null && _loadingTask.IsCompleted)
                {
                    _dataLoaded = DataLoader.IsLoaded;
                    _isLoadingData = false;

                    if (_dataLoaded)
                    {
                        Console.WriteLine("[RENDER] Minimal data loaded - App ready!");
                        Console.WriteLine("[RENDER] Starting background Pokemon loading...");
                        StartBackgroundLoading();
                    }
                    else
                    {
                        Console.WriteLine("[RENDER] Data loading failed");
                    }
                }

                return; // Don't render main UI yet
            }


            // Show background loading progress overlay
            if (!DataLoader.IsBackgroundLoadComplete)
            {
                RenderBackgroundLoadingOverlay();
            }
            // Main UI rendering starts here
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

                        // Show small Pokemon sprite if available
                        if (pokemonInSlot != null && !string.IsNullOrEmpty(pokemonInSlot.BaseData.Name))
                        {
                            var cachePath = $"cache/pokemon/{pokemonInSlot.BaseData.Name.ToLower()}.png";

                            // Download sprite if not cached
                            if (pokemonInSlot.BaseData.SpriteUrl != null && !File.Exists(cachePath))
                            {
                                _ = Task.Run(async () => await ImageUrlLoader.LoadImageFromUrl(pokemonInSlot.BaseData.SpriteUrl, cachePath));
                            }

                            // Display sprite
                            if (File.Exists(cachePath))
                            {
                                AddOrGetImagePointer(cachePath, true, out var spriteHandle, out _, out _);
                                ImGui.Image(spriteHandle, new Vector2(32, 32));
                                ImGui.SameLine();
                            }
                        }

                        ImGui.TextUnformatted(textToShow);

                        ImGui.TableSetColumnIndex(1);
                        if (pokemonInSlot == null)
                        {
                            if (ImGui.Button($"Add##{i}"))
                            {
                                _showPokemonEditorWindow = true;
                                _editingPosition = 0;  // 0 means new Pokemon
                                _editedBasePokemon = null;
                                _editedPokemonName = "";
                                _editedNickname = "";
                                _editedLevel = 50;
                            }
                        }
                        else
                        {
                            if (ImGui.Button($"Edit##{i}"))
                            {
                                _showPokemonEditorWindow = true;
                                _editedBasePokemon = pokemonInSlot.BaseData;

                                _editingPosition = i + 1;  // 1-6 for editing

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
                            if (ImGui.Button($"Del##{i}"))
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
                    // Pokemon Search (for NEW pokemon only)
                    if (_editedBasePokemon == null)
                    {
                        ImGui.Begin("Add New Pokemon");
                        ImGui.SeparatorText("Search Pokemon");

                        ImGui.SetNextItemWidth(300);
                        ImGui.InputText("Pokemon Name", ref _editedPokemonName, 64);

                        if (ImGui.Button("Search") || ImGui.IsKeyPressed(ImGuiKey.Enter))
                        {
                            string searchName = _editedPokemonName.Trim().ToLower();
                            if (!string.IsNullOrEmpty(searchName))
                            {
                                _pokemonSearchError = ""; // Clear previous error
                                var pokemon = PokemonService.GetPokemon(searchName).Result;
                                if (pokemon != null)
                                {
                                    _editedBasePokemon = pokemon;
                                    _editedNickname = char.ToUpper(pokemon.Name[0]) + pokemon.Name.Substring(1);
                                }
                                else
                                {
                                    _pokemonSearchError = $"Pokemon '{searchName}' not found!";
                                }
                            }
                        }

                        // Display persistent error message
                        if (!string.IsNullOrEmpty(_pokemonSearchError))
                        {
                            ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), _pokemonSearchError);
                        }

                        ImGui.End();
                    }

                    _editedIvs = new PokemonStats(
                        _editedIVsHP,
                        _editedIVsATTACK,
                        _editedIVsDEFENSE,
                        _editedIVsSPATTACK,
                        _editedIVsSPDEFENSE,
                        _editedIVsSPEED
                    );
                    _editedEvs = new PokemonStats(
                        _editedEVsHP,
                        _editedEVsATTACK,
                        _editedEVsDEFENSE,
                        _editedEVsSPATTACK,
                        _editedEVsSPDEFENSE,
                        _editedEVsSPEED
                    );

                    if (_editedBasePokemon != null)
                    {
                        ImGui.Begin("Edit Pokemon", ref _showPokemonEditorWindow);
                        ImGui.SeparatorText($"Editing: {_editedBasePokemon.Name}");

                        // ===== POKEMON SPRITE =====
                        if (_editedBasePokemon?.SpriteUrl != null)
                        {
                            var cachePath = $"cache/pokemon/{_editedBasePokemon.Name.ToLower()}.png";

                            // Download sprite if not cached
                            if (!File.Exists(cachePath))
                            {
                                _ = Task.Run(async () => await ImageUrlLoader.LoadImageFromUrl(_editedBasePokemon.SpriteUrl, cachePath));
                            }

                            // Display sprite
                            if (File.Exists(cachePath))
                            {
                                AddOrGetImagePointer(cachePath, true, out var spriteHandle, out _, out _);
                                ImGui.Image(spriteHandle, new Vector2(96, 96));
                                ImGui.SameLine();
                                ImGui.BeginGroup();
                                ImGui.Text(_editedBasePokemon.Name);
                                // Pokemon class doesn't have Level, only PlayerPokemon does
                                ImGui.EndGroup();
                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();
                            }
                        }

                        // Tab-based organization
                        if (ImGui.BeginTabBar("PokemonEditorTabs", ImGuiTabBarFlags.None))
                        {
                            // ==================== OVERVIEW TAB ====================
                            if (ImGui.BeginTabItem("Overview"))
                            {
                                ImGui.Spacing();
                                // Nickname
                                ImGui.InputText("Nickname", ref _editedNickname, 64);
                                // Level
                                if (ImGui.SliderInt("Level", ref _editedLevel, 1, 100))
                                {
                                    CalculateStatsRealTime();
                                }
                                // Gender
                                string[] genders = { "Male", "Female", "Genderless" };
                                ImGui.Combo("Gender", ref _editedGenderIndex, genders, genders.Length);

                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();
                                // Nature Selection
                                ImGui.Text("Nature:");
                                ImGui.SetNextItemWidth(250);
                                ImGui.InputText("##NatureSearch", ref _natureSearchFilter, 64);
                                ImGui.SameLine();
                                if (ImGui.Button("Clear##Nature"))
                                {
                                    _natureSearchFilter = "";
                                }
                                var natures = Nature.GetAllNatures();
                                var filteredNatures = string.IsNullOrWhiteSpace(_natureSearchFilter)
                                    ? natures
                                    : natures.Where(n => n.Name.Contains(_natureSearchFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                                string currentNature = _editedNatureIndex >= 0 && _editedNatureIndex < natures.Count
                                    ? natures[_editedNatureIndex].Name
                                    : "Select";
                                if (ImGui.BeginCombo("##NatureCombo", currentNature))
                                {
                                    foreach (var nature in filteredNatures)
                                    {
                                        int idx = natures.IndexOf(nature);
                                        bool isSelected = _editedNatureIndex == idx;
                                        if (ImGui.Selectable(nature.Name, isSelected))
                                        {
                                            _editedNatureIndex = idx;
                                            CalculateStatsRealTime();
                                        }
                                        if (isSelected)
                                            ImGui.SetItemDefaultFocus();
                                        // Tooltip
                                        if (ImGui.IsItemHovered())
                                        {
                                            ImGui.BeginTooltip();
                                            ImGui.TextUnformatted(nature.Name);
                                            if (nature.IsNeutral())
                                            {
                                                ImGui.TextUnformatted("Neutral (no stat changes)");
                                            }
                                            else
                                            {
                                                string increasedStatName = FormatStatName(nature.IncreasedStat ?? "");
                                                string decreasedStatName = FormatStatName(nature.DecreasedStat ?? "");
                                                if (!string.IsNullOrEmpty(increasedStatName))
                                                {
                                                    ImGui.TextUnformatted($"+10% {increasedStatName}");
                                                }
                                                if (!string.IsNullOrEmpty(decreasedStatName))
                                                {
                                                    ImGui.TextUnformatted($"-10% {decreasedStatName}");
                                                }
                                            }
                                            ImGui.EndTooltip();
                                        }
                                    }
                                    ImGui.EndCombo();
                                }
                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();
                                // Ability Selection
                                ImGui.Text("Ability:");
                                ImGui.SetNextItemWidth(250);
                                ImGui.InputText("##AbilitySearch", ref _abilitySearchFilter, 64);
                                ImGui.SameLine();
                                if (ImGui.Button("Clear##Ability"))
                                {
                                    _abilitySearchFilter = "";
                                }
                                // Use cached abilities (all ~300 from API)
                                var abilities = PokemonService.GetCachedAbilities() ?? Ability.GetCommonAbilities();
                                var filteredAbilities = string.IsNullOrWhiteSpace(_abilitySearchFilter)
                                    ? abilities
                                    : abilities.Where(a => a.Name.Contains(_abilitySearchFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                                string currentAbility = _editedAbilityIndex >= 0 && _editedAbilityIndex < abilities.Count
                                    ? abilities[_editedAbilityIndex].Name
                                    : "Select";
                                if (ImGui.BeginCombo("##AbilityCombo", currentAbility))
                                {
                                    foreach (var ability in filteredAbilities)
                                    {
                                        int idx = abilities.IndexOf(ability);
                                        bool isSelected = _editedAbilityIndex == idx;
                                        if (ImGui.Selectable(ability.Name, isSelected))
                                        {
                                            _editedAbilityIndex = idx;
                                        }
                                        if (ImGui.IsItemHovered())
                                        {
                                            // Fetch complete ability details (with effect)
                                            Console.WriteLine($"{ability.Name} - {ability.Description} - {ability.Effect}");
                                            Ability? abilityData = ability; // Start with cached basic data

                                            // Try to enhance with full API data if effect is missing
                                            if (string.IsNullOrEmpty(abilityData.Effect))
                                            {
                                                try
                                                {
                                                    var fullAbility = PokemonService.GetAbilityDetails(ability.Name).GetAwaiter().GetResult();
                                                    if (fullAbility != null && !string.IsNullOrEmpty(fullAbility.Effect))
                                                    {
                                                        abilityData = fullAbility;
                                                    }
                                                }
                                                catch
                                                {
                                                    // Continue with basic data
                                                }
                                            }

                                            ImGui.BeginTooltip();
                                            ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.4f, 1.0f), abilityData.Name);
                                            if (!string.IsNullOrEmpty(abilityData.Effect))
                                            {
                                                ImGui.PushTextWrapPos(320);
                                                ImGui.TextUnformatted(abilityData.Effect);
                                                ImGui.PopTextWrapPos();
                                            }
                                            ImGui.EndTooltip();
                                        }
                                    }
                                    ImGui.EndCombo();
                                }
                                ImGui.Spacing();
                                ImGui.Separator();
                                ImGui.Spacing();
                                // Item Selection
                                ImGui.Text("Held Item:");
                                ImGui.SetNextItemWidth(250);
                                ImGui.InputText("##ItemSearch", ref _itemSearchFilter, 64);
                                ImGui.SameLine();
                                if (ImGui.Button("Clear##Item"))
                                {
                                    _itemSearchFilter = "";
                                }
                                // Use cached items (pre-loaded at startup)
                                var items = PokemonService.GetCachedItems() ?? Item.GetCommonItems();
                                var filteredItems = string.IsNullOrWhiteSpace(_itemSearchFilter)
                                    ? items
                                    : items.Where(i => i.Name.Contains(_itemSearchFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                                string currentItem = _editedItemIndex >= 0 && _editedItemIndex < items.Count
                                    ? items[_editedItemIndex].Name
                                    : "None";
                                if (ImGui.BeginCombo("##ItemCombo", currentItem))
                                {
                                    foreach (var item in filteredItems)
                                    {
                                        int idx = items.IndexOf(item);
                                        bool isSelected = _editedItemIndex == idx;
                                        if (ImGui.Selectable(item.Name, isSelected))
                                        {
                                            _editedItemIndex = idx;
                                        }
                                        if (ImGui.IsItemHovered())
                                        {
                                            // Fetch complete item details on hover
                                            Item? itemDetails = null;
                                            try
                                            {
                                                itemDetails = PokemonService.GetItemDetails(item.Name).GetAwaiter().GetResult();
                                            }
                                            catch { }

                                            ImGui.BeginTooltip();

                                            // Show item sprite if available (48x48 in tooltip)
                                            bool spriteGroupOpened = false;
                                            if (itemDetails?.SpriteUrl != null)
                                            {
                                                var itemCachePath = $"cache/items/{itemDetails.Name.ToLower().Replace(" ", "-")}.png";

                                                // Download if not cached
                                                if (!File.Exists(itemCachePath))
                                                {
                                                    _ = Task.Run(async () => await ImageUrlLoader.LoadImageFromUrl(itemDetails.SpriteUrl, itemCachePath));
                                                }

                                                // Display sprite in tooltip
                                                if (File.Exists(itemCachePath))
                                                {
                                                    try
                                                    {
                                                        AddOrGetImagePointer(itemCachePath, true, out var itemHandle, out _, out _);
                                                        if (itemHandle != IntPtr.Zero)
                                                        {
                                                            ImGui.Image(itemHandle, new Vector2(48, 48));
                                                            ImGui.SameLine();
                                                            ImGui.BeginGroup();
                                                            spriteGroupOpened = true;
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        // Ignore errors - file might be being written or corrupted
                                                    }
                                                }
                                            }

                                            if (itemDetails != null)
                                            {
                                                // Item name (colored)
                                                ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.4f, 1.0f), itemDetails.Name);

                                                // Short description
                                                if (!string.IsNullOrEmpty(itemDetails.Description))
                                                {
                                                    ImGui.Spacing();
                                                    ImGui.PushTextWrapPos(300);
                                                    ImGui.TextUnformatted(itemDetails.Description);
                                                    ImGui.PopTextWrapPos();
                                                }

                                                ImGui.Separator();

                                                // Stats
                                                ImGui.TextUnformatted($"Category: {itemDetails.Category}");
                                                if (itemDetails.Cost > 0)
                                                    ImGui.TextUnformatted($"Cost: ₽{itemDetails.Cost}");
                                                if (itemDetails.FlingPower.HasValue && itemDetails.FlingPower.Value > 0)
                                                    ImGui.TextUnformatted($"Fling Power: {itemDetails.FlingPower}");

                                                // End sprite group if it was opened
                                                if (spriteGroupOpened)
                                                {
                                                    ImGui.EndGroup();
                                                }
                                            }
                                            else
                                            {
                                                // Fallback if API call failed
                                                ImGui.TextUnformatted(item.Name);
                                                if (!string.IsNullOrEmpty(item.Effect))
                                                {
                                                    ImGui.TextUnformatted(item.Effect);
                                                }
                                            }

                                            ImGui.EndTooltip();
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                ImGui.EndTabItem();
                            }
                            // ==================== STATS TAB ====================
                            if (ImGui.BeginTabItem("Stats"))
                            {
                                ImGui.Spacing();
                                // IVs Section (no TreeNode)
                                ImGui.TextColored(new Vector4(0.4f, 0.8f, 1.0f, 1.0f), "Individual Values (IVs) - Range: 0-31");
                                ImGui.Separator();
                                ImGui.Spacing();
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
                                ImGui.Spacing();
                                ImGui.Spacing();
                                // EVs Section (no TreeNode)
                                int totalEVs = StatsCalculator.GetTotalEVs(_editedEvs);
                                ImGui.TextColored(new Vector4(0.4f, 1.0f, 0.4f, 1.0f), $"Effort Values (EVs) - Total: {totalEVs}/510");
                                ImGui.Separator();
                                ImGui.Spacing();
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
                                CalculateStatsRealTime();
                                ImGui.Spacing();
                                ImGui.Spacing();
                                // Calculated Stats Section (no TreeNode)
                                ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.4f, 1.0f), "Final Calculated Stats");
                                ImGui.Separator();
                                ImGui.Spacing();
                                ImGui.Text($"HP: {_editedCalculatedStats.HP}");
                                ImGui.Text($"Attack: {_editedCalculatedStats.Attack}");
                                ImGui.Text($"Defense: {_editedCalculatedStats.Defense}");
                                ImGui.Text($"Sp.Atk: {_editedCalculatedStats.SpecialAttack}");
                                ImGui.Text($"Sp.Def: {_editedCalculatedStats.SpecialDefense}");
                                ImGui.Text($"Speed: {_editedCalculatedStats.Speed}");
                                ImGui.EndTabItem();
                            }
                            // ==================== TYPES TAB ====================
                            if (ImGui.BeginTabItem("Types"))
                            {
                                ImGui.Spacing();

                                // Pokemon Type & Effectiveness Display
                                if (_editedBasePokemon != null)
                                {
                                    RenderPokemonTypeInfo(_editedBasePokemon);
                                }

                                ImGui.EndTabItem();
                            }
                            // ==================== MOVESET TAB ====================
                            if (ImGui.BeginTabItem("Moveset"))
                            {
                                ImGui.Spacing();
                                // Moveset editor (removed TreeNode wrapper)
                                string[] availableMoveNames = _editedBasePokemon.Moves?.Select(m => m.Name).ToArray() ?? Array.Empty<string>();
                                for (int i = 0; i < 4; i++)
                                {
                                    string currentMove = i < _selectedMoves.Count
                                        ? _selectedMoves[i].MoveData.Name
                                        : "Empty";
                                    // Individual search bar for each move
                                    ImGui.Text($"Move {i + 1}:");
                                    ImGui.SetNextItemWidth(250);
                                    ImGui.InputText($"##MoveSearch{i}", ref _moveSearchFilters[i], 64);
                                    ImGui.SameLine();
                                    if (ImGui.Button($"Clear##Move{i}"))
                                    {
                                        _moveSearchFilters[i] = "";
                                    }
                                    // Filter moves based on individual search
                                    var filteredMoves = string.IsNullOrWhiteSpace(_moveSearchFilters[i])
                                        ? availableMoveNames
                                        : availableMoveNames.Where(m => m.Contains(_moveSearchFilters[i], StringComparison.OrdinalIgnoreCase)).ToArray();
                                    if (ImGui.BeginCombo($"##MoveCombo{i}", currentMove))
                                    {
                                        foreach (var moveName in filteredMoves)
                                        {
                                            bool isSelected = currentMove == moveName;
                                            if (ImGui.Selectable(moveName, isSelected))
                                            {
                                                var moveData = _editedBasePokemon.Moves.FirstOrDefault(m => m.Name == moveName);
                                                if (moveData != null)
                                                {
                                                    var learnedMove = new LearnedMove(moveData);

                                                    // Replace move at this slot
                                                    if (i < _selectedMoves.Count)
                                                        _selectedMoves[i] = learnedMove;
                                                    else
                                                        _selectedMoves.Add(learnedMove);
                                                }
                                            }
                                            // Tooltip with move info
                                            if (ImGui.IsItemHovered())
                                            {
                                                // Use cached move data first (instant, no lag)
                                                PokemonMove? moveData = _editedBasePokemon.Moves?.FirstOrDefault(m => m.Name == moveName);

                                                // If basic data exists but incomplete, try to enhance from API cache (non-blocking)
                                                if (moveData != null && string.IsNullOrEmpty(moveData.TypeName))
                                                {
                                                    try
                                                    {
                                                        // Non-blocking: check if already in move cache
                                                        var cachedMove = PokemonService.GetMoveDetails(moveName).GetAwaiter().GetResult();
                                                        if (cachedMove != null)
                                                        {
                                                            moveData = cachedMove;
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        // Silently continue with basic data
                                                        Console.WriteLine($"Failed to enhance move '{moveName}': {ex.Message}");
                                                    }
                                                }

                                                if (moveData != null)
                                                {
                                                    ImGui.BeginTooltip();
                                                    ImGui.PushTextWrapPos(320); // Set wrap width for entire tooltip

                                                    // ===== PHASE 2: Header with colored type name =====
                                                    if (!string.IsNullOrEmpty(moveData.TypeName))
                                                    {
                                                        RenderMoveTypeIcon(moveData.TypeName, new Vector2(20, 20));
                                                        ImGui.SameLine();
                                                        ImGui.TextColored(GetTypeColor(moveData.TypeName), moveName);
                                                    }
                                                    else
                                                    {
                                                        ImGui.TextUnformatted(moveName);
                                                    }

                                                    ImGui.Spacing();

                                                    // ===== PHASE 1: Damage Class Badge =====
                                                    RenderDamageClassBadge(moveData.DamageClass);

                                                    // ===== PHASE 2: Description =====
                                                    if (!string.IsNullOrEmpty(moveData.Description))
                                                    {
                                                        ImGui.Spacing();
                                                        ImGui.Separator();
                                                        ImGui.Spacing();
                                                        ImGui.TextWrapped(moveData.Description);
                                                    }

                                                    ImGui.Spacing();
                                                    ImGui.Separator();
                                                    ImGui.Spacing();

                                                    // ===== PHASE 1 & 3: Power with color and STAB =====
                                                    if (moveData.Power > 0)
                                                    {
                                                        var powerColor = GetPowerColor(moveData.Power);
                                                        var stabPower = CalculateSTABPower(moveData, _editedBasePokemon);

                                                        if (stabPower.HasValue)
                                                        {
                                                            ImGui.TextColored(powerColor, $"Power: {moveData.Power}");
                                                            ImGui.SameLine();
                                                            ImGui.TextColored(new Vector4(0.3f, 1.0f, 0.3f, 1.0f), $"({stabPower} with STAB)");
                                                        }
                                                        else
                                                        {
                                                            ImGui.TextColored(powerColor, $"Power: {moveData.Power}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var powerColor = GetPowerColor(0);
                                                        ImGui.TextColored(powerColor, "Power: -");
                                                    }

                                                    // ===== PHASE 1: PP and Accuracy with icons =====
                                                    ImGui.Text($"PP: {moveData.PP}");
                                                    ImGui.SameLine();
                                                    ImGui.Text("   "); // Spacing
                                                    ImGui.SameLine();
                                                    if (moveData.Accuracy > 0)
                                                    {
                                                        ImGui.Text($"Accuracy: {moveData.Accuracy}%");
                                                    }
                                                    else
                                                    {
                                                        ImGui.Text("Accuracy: -");
                                                    }

                                                    // ===== Priority (only if non-zero) =====
                                                    if (moveData.Priority != 0)
                                                    {
                                                        var priorityColor = moveData.Priority > 0
                                                            ? new Vector4(0.3f, 1.0f, 0.3f, 1.0f)  // Green for +priority
                                                            : new Vector4(1.0f, 0.5f, 0.3f, 1.0f); // Orange for -priority
                                                        ImGui.TextColored(priorityColor, $"Priority: {(moveData.Priority > 0 ? "+" : "")}{moveData.Priority}");
                                                    }

                                                    // ===== PHASE 1: Target info =====
                                                    ImGui.Text(FormatMoveTarget(moveData.Target));

                                                    // ===== PHASE 3: Type Effectiveness Preview =====
                                                    if (!string.IsNullOrEmpty(moveData.TypeName) && moveData.Power > 0)
                                                    {
                                                        ImGui.Spacing();
                                                        ImGui.Separator();
                                                        ImGui.Spacing();
                                                        RenderEffectivenessPreview(moveData.TypeName, 320);
                                                    }

                                                    ImGui.PopTextWrapPos();
                                                    ImGui.EndTooltip();
                                                }
                                            }
                                        }
                                        ImGui.EndCombo();
                                    }
                                    ImGui.Spacing();
                                }
                                ImGui.EndTabItem();
                            }

                            // ==================== BASE STATS TAB ====================
                            if (ImGui.BeginTabItem("Base Stats"))
                            {
                                ImGui.Spacing();
                                ImGui.TextColored(new Vector4(0.4f, 0.8f, 1.0f, 1.0f), "Base Stats (Species Stats)");
                                ImGui.Separator();
                                ImGui.Spacing();

                                if (_editedBasePokemon?.BaseStats != null)
                                {
                                    // Stat names from PokeAPI
                                    var statNames = new[] { "hp", "attack", "defense", "special-attack", "special-defense", "speed" };
                                    var statLabels = new[] { "HP", "Attack", "Defense", "Sp. Atk", "Sp. Def", "Speed" };

                                    for (int i = 0; i < statNames.Length; i++)
                                    {
                                        var statName = statNames[i];
                                        var label = statLabels[i];
                                        var value = _editedBasePokemon.BaseStats.HP; // placeholder - will use proper lookup

                                        // For now, show placeholder data
                                        if (statName == "hp") value = _editedBasePokemon.BaseStats.HP;
                                        else if (statName == "attack") value = _editedBasePokemon.BaseStats.Attack;
                                        else if (statName == "defense") value = _editedBasePokemon.BaseStats.Defense;
                                        else if (statName == "special-attack") value = _editedBasePokemon.BaseStats.SpecialAttack;
                                        else if (statName == "special-defense") value = _editedBasePokemon.BaseStats.SpecialDefense;
                                        else if (statName == "speed") value = _editedBasePokemon.BaseStats.Speed;

                                        // Label
                                        ImGui.Text($"{label}:");
                                        ImGui.SameLine(120);

                                        // Value
                                        ImGui.Text($"{value}");
                                        ImGui.SameLine(180);

                                        // Progress bar (relative to 255 max)
                                        var percent = value / 255f;
                                        var color = GetStatColor(value);

                                        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, color);
                                        ImGui.ProgressBar(percent, new Vector2(300, 22), "");
                                        ImGui.PopStyleColor();
                                    }

                                    // Total base stat
                                    var total = _editedBasePokemon.BaseStats.HP +
                                               _editedBasePokemon.BaseStats.Attack +
                                               _editedBasePokemon.BaseStats.Defense +
                                               _editedBasePokemon.BaseStats.SpecialAttack +
                                               _editedBasePokemon.BaseStats.SpecialDefense +
                                               _editedBasePokemon.BaseStats.Speed;

                                    ImGui.Spacing();
                                    ImGui.Separator();
                                    ImGui.Spacing();
                                    ImGui.TextColored(new Vector4(1, 1, 0.3f, 1), $"Total Base Stat: {total}");
                                }
                                else
                                {
                                    ImGui.TextColored(new Vector4(1, 0.5f, 0, 1), "⚠️ Base stats not loaded");
                                    ImGui.Text("Stats will load when Pokemon is fully cached.");
                                }

                                ImGui.EndTabItem();
                            }

                            ImGui.EndTabBar();
                        }
                        ImGui.Spacing();
                        // Save/Cancel buttons (outside tabs, always visible)
                        if (ImGui.Button("Save"))
                        {
                            SavePokemonEditor();
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel"))
                        {
                            _showPokemonEditorWindow = false;
                            _editedBasePokemon = null;
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

        private void StartBackgroundLoading()
        {
            if (_backgroundLoadingTask != null) return; // Already started

            var progress = new Progress<LoadingProgress>(p =>
            {
                _backgroundProgress = p; // Update for overlay rendering
            });

            _backgroundLoadingTask = Task.Run(async () =>
                await DataLoader.LoadBackgroundData(progress));
        }

        private void RenderBackgroundLoadingOverlay()
        {
            if (DataLoader.IsBackgroundLoadComplete) return;

            // Semi-transparent overlay at bottom-right
            var windowSize = ImGui.GetIO().DisplaySize;
            ImGui.SetNextWindowPos(new Vector2(windowSize.X - 420, windowSize.Y - 100));
            ImGui.SetNextWindowSize(new Vector2(400, 90));
            ImGui.SetNextWindowBgAlpha(0.95f);

            ImGui.Begin("##BackgroundLoading",
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoCollapse);

            ImGui.TextColored(new Vector4(0.4f, 0.8f, 1.0f, 1.0f), "📦 Loading Pokemon Data...");

            if (_backgroundProgress != null)
            {
                var percent = (float)(_backgroundProgress.Percentage / 100.0);
                ImGui.ProgressBar(percent, new Vector2(-1, 20),
                    $"{_backgroundProgress.Current}/{_backgroundProgress.Total} ({_backgroundProgress.Percentage:F1}%)");

                if (_backgroundProgress.EstimatedTimeRemaining.HasValue)
                {
                    var eta = _backgroundProgress.EstimatedTimeRemaining.Value;
                    var hours = eta.Hours;
                    var minutes = eta.Minutes;

                    if (hours > 0)
                        ImGui.Text($"⏱️ ~{hours}h {minutes}m remaining");
                    else
                        ImGui.Text($"⏱️ ~{minutes}m remaining");
                }
            }
            else
            {
                ImGui.ProgressBar(0f, new Vector2(-1, 20), "Starting...");
            }

            ImGui.End();
        }

        private Vector4 GetStatColor(int statValue)
        {
            // Color coding for base stats
            if (statValue >= 150) return new Vector4(0, 1, 0, 1);        // Bright green (excellent)
            if (statValue >= 100) return new Vector4(0.5f, 1, 0, 1);     // Yellow-green (good)
            if (statValue >= 70) return new Vector4(1, 1, 0, 1);         // Yellow (average)
            if (statValue >= 50) return new Vector4(1, 0.7f, 0, 1);      // Orange (below average)
            return new Vector4(1, 0.3f, 0, 1);                            // Red-orange (poor)
        }

        private IntPtr GetTypeImageHandle(string typeName)
        {
            return typeName.ToLower() switch
            {
                "fire" => FireTypeImageHandle,
                "water" => WaterTypeImageHandle,
                "grass" => GrassTypeImageHandle,
                "electric" => ElectricTypeImageHandle,
                "normal" => NormalTypeImageHandle,
                "fighting" => FightingTypeImageHandle,
                "flying" => FlyingTypeImageHandle,
                "poison" => PoisonTypeImageHandle,
                "ground" => GroundTypeImageHandle,
                "rock" => RockTypeImageHandle,
                "bug" => BugTypeImageHandle,
                "ghost" => GhostTypeImageHandle,
                "steel" => SteelTypeImageHandle,
                "psychic" => PsychicTypeImageHandle,
                "ice" => IceTypeImageHandle,
                "dragon" => DragonTypeImageHandle,
                "dark" => DarkTypeImageHandle,
                "fairy" => FairyTypeImageHandle,
                _ => IntPtr.Zero
            };
        }
    }
}
