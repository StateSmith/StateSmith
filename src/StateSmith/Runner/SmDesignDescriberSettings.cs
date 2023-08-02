#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/200
/// </summary>
public class SmDesignDescriberSettings
{
    public bool enabled = false;

    public Sections outputSections = new();

    /// <summary>
    /// Optional. Can be relative (to normal output path) or absolute.
    /// </summary>
    public string? outputDirectory;

    public class Sections
    {
        /// <summary>
        /// Set to false to disable outputting before transformations.
        /// Before transformations is good for when you want to see the original design.
        /// </summary>
        public bool beforeTransformations = true;

        /// <summary>
        /// Set to false to disable outputting after transformations.
        /// After transformations is good for when you want to see what is passed to the code generator.
        /// This is especially useful when you want to understand transformation steps like
        /// TriggerMaps and other custom transformations.
        /// </summary>
        public bool afterTransformations = true;
    }
}
