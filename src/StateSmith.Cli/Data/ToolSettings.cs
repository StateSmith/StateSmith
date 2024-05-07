using System;

namespace StateSmith.Cli.Data;

/// <summary>
/// stored in json format.
/// </summary>
public class ToolSettings
{
    /// <summary>
    /// Set to -1 to disable update checks.
    /// </summary>
    public double SecondsBetweenUpdateChecks { get; set; } = TimeSpan.FromDays(1).TotalSeconds;
}
