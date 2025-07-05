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

    public RunnerContext()
    {
        runnerSettings = new();
        renderConfigAllVars = new();
        renderConfig = new DummyIRenderConfig();
    }
}
