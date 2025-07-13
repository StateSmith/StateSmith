#nullable enable

using StateSmith.Output.UserConfig;
using StateSmith.Output.UserConfig.AutoVars;

namespace StateSmith.Runner;

/// <summary>
/// This context object stores the runtime configuration for a given run.
/// Automatically generates RenderConfigAllVars from the renderConfig and runnerSettings.
/// </summary>
public class RunnerContext
{
    public RenderConfigAllVars renderConfigAllVars;
    
    public RunnerSettings runnerSettings
    {
        get => _runnerSettings;
        set
        {
            var tmp = _runnerSettings.autoDeIndentAndTrimRenderConfigItems;
            _runnerSettings = value;
            if (tmp != value.autoDeIndentAndTrimRenderConfigItems)
            {
                // If the autoDeIndentAndTrimRenderConfigItems setting changes, we need to update the renderConfigAllVars
                renderConfigAllVars.SetFrom(renderConfig, _runnerSettings.autoDeIndentAndTrimRenderConfigItems);
            }
        }
    }

    public IRenderConfig renderConfig
    {
        get => _renderConfig;
        set
        {
            _renderConfig = value;
            renderConfigAllVars.SetFrom(value, runnerSettings.autoDeIndentAndTrimRenderConfigItems);
        }
    }

    private RunnerSettings _runnerSettings;
    private IRenderConfig _renderConfig;

    /// The path to the file that called a <see cref="SmRunner"/> constructor. Allows for convenient relative path
    /// figuring for regular C# projects and C# scripts (.csx).
    /// May be null during construction but is expected to be non-null at the time of Run
    public string? callerFilePath;

    public RunnerContext()
    {
        _runnerSettings = new();
        renderConfigAllVars = new();
        _renderConfig = new DummyIRenderConfig();
    }
}
