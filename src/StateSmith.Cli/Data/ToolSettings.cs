namespace StateSmith.Cli.Data;

/// <summary>
/// stored in json format.
/// </summary>
public class ToolSettings
{
    public bool RemindForUpdates { get; set; } = true;
    public int MilliSecondsBetweenUpdateChecks { get; set; } = 1000 * 60 * 60 * 24; // 24 hours
}
