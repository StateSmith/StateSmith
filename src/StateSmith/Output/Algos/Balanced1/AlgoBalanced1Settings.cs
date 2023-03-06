#nullable enable


namespace StateSmith.Output.Algos.Balanced1;

// Useful info: https://github.com/StateSmith/StateSmith/wiki/Multiple-Language-Support

public class AlgoBalanced1Settings
{
    // Used for C like stuff that has to hoist stuff out of class.
    // In the future, it would be nice to remove this and just run a code formatter for the transpilers
    // that don't want the class indentation.
    public bool skipClassIndentation = true;
}
