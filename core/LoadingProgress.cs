using System;

namespace PasaporteFiller.core;

/// <summary>
/// Tracks progress of background data loading
/// </summary>
public class LoadingProgress
{
    public int Current { get; set; }
    public int Total { get; set; }
    public string Phase { get; set; } = "";

    public double Percentage => Total > 0 ? (Current * 100.0 / Total) : 0;

    /// <summary>
    /// Estimated time remaining based on average load time
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Formatted status message for display
    /// </summary>
    public string StatusMessage => $"{Phase}: {Current}/{Total} ({Percentage:F1}%)";
}
