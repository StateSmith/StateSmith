#nullable enable

namespace StateSmith.Runner;

public class DrawIoSettings
{
    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/81
    /// Might want to disable if the detection algorithm fails at a case we hadn't considered.
    /// </summary>
    public bool checkForChildStateContainment = true;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/81
    /// Might want to disable if the detection algorithm fails at a case we hadn't considered.
    /// </summary>
    public bool checkForBadOverlap = true;
}
