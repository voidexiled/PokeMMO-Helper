using System.Drawing;

namespace PasaporteFiller.core;

/// <summary>
/// Application settings structure
/// </summary>
public class AppSettings
{
    public OCRSettings OCR { get; set; }
    public UISettings UI { get; set; }
    public BattleSettings Battle { get; set; }
    
    public AppSettings()
    {
        OCR = new OCRSettings();
        UI = new UISettings();
        Battle = new BattleSettings();
    }
}

/// <summary>
/// OCR and scanning settings
/// </summary>
public class OCRSettings
{
    public int ScanInterval { get; set; } = 5000;  // milliseconds
    public bool UseLevenshtein { get; set; } = true;
    public Dictionary<string, Rectangle> ScanAreas { get; set; } = new();
    public string TesseractLanguage { get; set; } = "eng";
    
    public OCRSettings()
    {
        // Default scan areas (can be customized by user)
        ScanAreas = new Dictionary<string, Rectangle>
        {
            { "OpponentName", new Rectangle(900, 100, 300, 50) },
            { "OpponentLevel", new Rectangle(1100, 150, 80, 30) },
            { "OpponentHP", new Rectangle(950, 180, 200, 20) }
        };
    }
}

/// <summary>
/// UI appearance settings
/// </summary>
public class UISettings
{
    public float Opacity { get; set; } = 0.95f;
    public bool TransparentBackground { get; set; } = false;
    public bool ShowWeaknesses { get; set; } = true;
    public bool ShowStrengths { get; set; } = false;
    public bool ShowNormalEffectiveness { get; set; } = false;
    
    // Window positions (saved for convenience)
    public Dictionary<string, (int X, int Y)> WindowPositions { get; set; } = new();
}

/// <summary>
/// Battle assistant settings
/// </summary>
public class BattleSettings
{
    public bool EnableBattleAssistant { get; set; } = true;
    public bool AutoDetectBattle { get; set; } = true;
    public bool ShowDamageCalculations { get; set; } = true;
    public bool ShowSwitchRecommendations { get; set; } = true;
    public bool ShowMoveEffectiveness { get; set; } = true;
    
    // Advanced
    public bool ConsiderAbilities { get; set; } = false;  // Future enhancement
    public bool ConsiderWeather { get; set; } = false;    // Future enhancement
    public bool ConsiderItems { get; set; } = false;      // Future enhancement
}
