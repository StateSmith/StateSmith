using StateSmith.Output.C99BalancedCoder1;
using StateSmith.Output.UserConfig;
using System;

#nullable enable

namespace StateSmith.Runner;

public class RunnerSettings
{
    public IRenderConfigC renderConfig;

    public string diagramFile;

    public string? outputDirectory;

    /// <summary>
    /// Only required if multiple state machines present in diagram file.
    /// </summary>
    public string? stateMachineName;

    /// <summary>
    /// Optional. This is used to control how file paths are printed.
    /// Set to an empty string "" if you want the full absolute path to be printed.
    /// https://github.com/StateSmith/StateSmith/issues/79
    /// </summary>
    public string? filePathPrintBase;

    public CodeStyleSettings style = new();

    public CNameMangler mangler = new();

    public readonly DrawIoSettings drawIoSettings = new();

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/82
    /// </summary>
    public bool dumpErrorsToFile = false;

    /// <summary>
    /// If false (the default), any exception thrown while running StateSmith will be caught
    /// and printed with details. If true, the original exception will propagate with its
    /// original stack trace.
    /// </summary>
    public bool propagateExceptions = false;

    public bool autoDeIndentAndTrimRenderConfigItems = true;

    public RunnerSettings(IRenderConfigC renderConfig, string diagramFile, string? outputDirectory = null)
    {
        this.renderConfig = renderConfig;
        this.diagramFile = diagramFile;
        this.outputDirectory = outputDirectory;
    }
}
