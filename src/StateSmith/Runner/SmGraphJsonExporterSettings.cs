#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/528
/// </summary>
public class SmGraphJsonExporterSettings
{
    public bool enabled = false;

    /// <summary>
    /// Optional. Can be relative (to normal output path) or absolute.
    /// </summary>
    public string? outputDirectory;

    public string outputFileNamePostfix = ".export.json";

    /// <summary>
    /// Before transformations is good for when you want to see the original design before
    /// various transformations/optimizations are performed on state machine graph.
    /// Note that state names are not necessarily unique until after transformations are run.
    /// </summary>
    public bool beforeTransformations = true;

    /// <summary>
    /// After transformations is good for when you want to see what is passed to the code generator.
    /// This is especially useful when you want to understand transformation steps like
    /// TriggerMaps and other custom transformations or optimizations.
    /// </summary>
    public bool afterTransformations = true;
}
