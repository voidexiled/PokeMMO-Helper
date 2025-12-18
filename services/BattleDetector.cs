using PasaporteFiller.core;

namespace PasaporteFiller.services;

/// <summary>
/// Detects and tracks battle state using OCR and screen recognition
/// </summary>
public static class BattleDetector
{
    private static BattleState _currentBattle = new();
    private static readonly object _battleLock = new();

    /// <summary>
    /// Gets the current battle state
    /// </summary>
    public static BattleState CurrentBattle
    {
        get
        {
            lock (_battleLock)
            {
                return _currentBattle;
            }
        }
    }

    /// <summary>
    /// Gets whether currently in a battle
    /// </summary>
    public static bool IsInBattle
    {
        get
        {
            lock (_battleLock)
            {
                return _currentBattle.IsInBattle;
            }
        }
    }

    /// <summary>
    /// Updates battle state from OCR (placeholder - implement with actual OCR logic)
    /// </summary>
    public static void UpdateBattleState()
    {
        lock (_battleLock)
        {
            // TODO: Integrate with ScreenTextRecognizer to detect:
            // - Opponent Pokemon name
            // - Opponent level  
            // - Opponent HP bar percentage
            //
            // For now, this is a placeholder that can be expanded later
            // when implementing the UI

            bool battleDetected = DetectBattle();

            if (battleDetected && !_currentBattle.IsInBattle)
            {
                // Battle just started
                _currentBattle.IsInBattle = true;
                _currentBattle.BattleStartTime = DateTime.Now;
            }
            else if (!battleDetected && _currentBattle.IsInBattle)
            {
                // Battle ended
                ResetBattle();
            }
        }
    }

    /// <summary>
    /// Detects if currently in a battle (placeholder)
    /// </summary>
    public static bool DetectBattle()
    {
        // TODO: Implement actual battle detection logic
        // This could check for specific UI elements or text patterns
        // that indicate a battle is in progress
        // 
        // For now, returns current state
        lock (_battleLock)
        {
            return _currentBattle.IsInBattle;
        }
    }

    /// <summary>
    /// Manually sets battle state (for UI integration)
    /// </summary>
    public static void SetBattleState(
        PlayerPokemon? playerPokemon,
        Pokemon? opponentPokemon,
        int opponentLevel = 50,
        int opponentCurrentHP = 0,
        int opponentMaxHP = 0)
    {
        lock (_battleLock)
        {
            _currentBattle.CurrentPlayerPokemon = playerPokemon;
            _currentBattle.OpponentPokemon = opponentPokemon;
            _currentBattle.OpponentLevel = opponentLevel;
            _currentBattle.OpponentCurrentHP = opponentCurrentHP;
            _currentBattle.OpponentMaxHP = opponentMaxHP;
            _currentBattle.IsInBattle = (playerPokemon != null && opponentPokemon != null);

            if (_currentBattle.IsInBattle)
            {
                _currentBattle.BattleStartTime = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// Resets the battle state
    /// </summary>
    public static void ResetBattle()
    {
        lock (_battleLock)
        {
            _currentBattle.Reset();
        }
    }

    /// <summary>
    /// Gets opponent name from OCR (placeholder)
    /// </summary>
    public static string? GetOpponentName()
    {
        // TODO: Integrate with ScreenTextRecognizer
        // to extract opponent Pokemon name from specific screen region

        lock (_battleLock)
        {
            return _currentBattle.OpponentPokemon?.Name;
        }
    }

    /// <summary>
    /// Gets opponent level from OCR (placeholder)
    /// </summary>
    public static int GetOpponentLevel()
    {
        // TODO: Integrate with ScreenTextRecognizer
        // to extract level number from screen

        lock (_battleLock)
        {
            return _currentBattle.OpponentLevel;
        }
    }

    /// <summary>
    /// Gets opponent HP percentage from visual recognition (placeholder)
    /// </summary>
    public static float GetOpponentHPPercentage()
    {
        // TODO: Implement HP bar pixel analysis
        // Read HP bar color/width to estimate HP percentage

        lock (_battleLock)
        {
            return _currentBattle.OpponentHPPercentage;
        }
    }
}
