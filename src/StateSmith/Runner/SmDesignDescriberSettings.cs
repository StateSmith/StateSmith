#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/200
/// </summary>
public class SmDesignDescriberSettings
{
    public bool enabled = false;

    /// <summary>
    /// Optional. Can be relative (to normal output path) or absolute.
    /// </summary>
    public string? outputDirectory;
}
