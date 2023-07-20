#nullable enable

namespace StateSmith.Runner;

public class SmDescriberSettings
{
    public bool enabled = false;

    /// <summary>
    /// Optional. Can be relative (to normal output path) or absolute.
    /// </summary>
    public string? outputDirectory;
}
