#nullable enable

namespace StateSmith.Runner;

public class SimulationSettings
{
    public bool enableGeneration = false;

    /// <summary>
    /// Defaults to same directory as <see cref="RunnerSettings.outputDirectory"/>.
    /// </summary>
    public string? outputDirectory = null;
    
    public string outputFileNamePostfix = ".sim.html";
}
