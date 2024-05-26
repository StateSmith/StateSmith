#nullable enable

namespace StateSmith.Runner;

public class SimulationSettings
{
    public bool enableGeneration = true;

    /// <summary>
    /// Defaults to same directory as <see cref="RunnerSettings.outputDirectory"/>.
    /// </summary>
    public string? outputDirectory = null;

    /// <summary>
    /// Defaults to state machine name + ".sim.html"
    /// </summary>
    public string? outputFileName = null;
}
