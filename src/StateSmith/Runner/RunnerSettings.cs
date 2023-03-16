using StateSmith.Output;
using StateSmith.Output.UserConfig;
using System;

#nullable enable

namespace StateSmith.Runner;

public class RunnerSettings
{
    /// <summary>
    /// todo_low rename to diagramPath to better reflect its purpose.
    /// </summary>
    public string diagramFile;

    public string? outputDirectory;

    /// <summary>
    /// Ignored if custom code generator used.
    /// </summary>
    public AlgorithmId algorithmId = AlgorithmId.Default;

    /// <summary>
    /// Ignored if custom code generator used.
    /// </summary>
    public TranspilerId transpilerId = TranspilerId.Default;

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

    public readonly DrawIoSettings drawIoSettings = new();

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/82
    /// </summary>
    public bool dumpErrorsToFile = false;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/wiki/GIL:-Generic-Intermediate-Language
    /// </summary>
    public bool dumpGilCodeOnError = false;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/wiki/GIL:-Generic-Intermediate-Language
    /// </summary>
    public bool outputGilCodeAlways = false;

    /// <summary>
    /// If false (the default), any exception thrown while running StateSmith will be caught
    /// and printed with details. If true, the original exception will propagate with its
    /// original stack trace.
    /// </summary>
    public bool propagateExceptions = false;

    public bool autoDeIndentAndTrimRenderConfigItems = true;

    /// <summary>
    /// See https://github.com/StateSmith/StateSmith/issues/138
    /// </summary>
    public NameConflictResolution nameConflictResolution = NameConflictResolution.Manual;

    public RunnerSettings(string diagramFile, string? outputDirectory = null, AlgorithmId algorithmId = default, TranspilerId transpilerId = default)
    {
        this.diagramFile = diagramFile;
        this.outputDirectory = outputDirectory;
        this.algorithmId = algorithmId;
        this.transpilerId = transpilerId;
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/138
    /// </summary>
    public enum NameConflictResolution
    {
        Manual,
        ShortFqnAncestor,
        ShortFqnParent
    }
}
