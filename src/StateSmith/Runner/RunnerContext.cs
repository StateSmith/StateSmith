#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Output.UserConfig.AutoVars;

namespace StateSmith.Runner;

/// <summary>
/// This context object stores the runtime configuration for a given run
/// of SmRunner.
/// </summary>
public class RunnerContext
{
    public RunnerSettings runnerSettings;
    public RenderConfigAllVars renderConfigAllVars;
    public IRenderConfig renderConfig;

    /// The path to the file that called a <see cref="SmRunner"/> constructor. Allows for convenient relative path
    /// figuring for regular C# projects and C# scripts (.csx).
    /// May be null during construction but is expected to be non-null at the time of Run
    public string? callerFilePath;

    public RunnerContext()
    {
        runnerSettings = new();
        renderConfigAllVars = new();
        renderConfig = new DummyIRenderConfig();
    }
}
