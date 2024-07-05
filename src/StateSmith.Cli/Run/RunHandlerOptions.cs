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

    public readonly string CurrentDirectory;

    public RunHandlerOptions(string currentDirectory)
    {
        CurrentDirectory = currentDirectory;
    }
}

