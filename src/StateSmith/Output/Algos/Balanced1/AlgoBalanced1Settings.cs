#nullable enable


namespace StateSmith.Output.Algos.Balanced1;

// Useful info: https://github.com/StateSmith/StateSmith/wiki/Multiple-Language-Support

/// <summary>
/// Algorithm Balanced 1 Settings.
/// https://github.com/StateSmith/StateSmith/wiki/Algorithms
/// </summary>
public class AlgoBalanced1Settings
{
    /// <summary>
    /// Used for C like stuff that has to hoist stuff out of class.
    /// In the future, it would be nice to remove this and just run a code formatter for the transpilers
    /// that don't want the class indentation.
    /// </summary>
    internal bool skipClassIndentation = true;

    /// <summary>
    /// Set to false if don't want the event ID to string function to be generated and output.
    /// https://github.com/StateSmith/StateSmith/issues/181
    /// </summary>
    public bool outputEventIdToStringFunction = true;

    /// <summary>
    /// Set to false if don't want the state ID to string function to be generated and output.
    /// https://github.com/StateSmith/StateSmith/issues/181
    /// </summary>
    public bool outputStateIdToStringFunction = true;
}
