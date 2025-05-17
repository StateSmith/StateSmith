using System.Collections.Generic;

namespace StateSmith.Cli.Run;

public class RunHandlerOptions
{
    public bool Verbose;
    public bool NoCsx;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/348
    /// </summary>
    public bool PropagateExceptions;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/348
    /// </summary>
    public bool DumpErrorsToFile;
    public bool Rebuild;
    public bool Watch;

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/428
    /// </summary>
    public IEnumerable<string> ConfigFiles = new List<string>();

    public readonly string CurrentDirectory;

    public RunHandlerOptions(string currentDirectory)
    {
        CurrentDirectory = currentDirectory;
    }
}

