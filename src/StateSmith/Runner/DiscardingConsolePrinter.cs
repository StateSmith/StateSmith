namespace StateSmith.Runner;

// we may want to capture this info at some point
public class DiscardingConsolePrinter : IConsolePrinter
{
    public void SetLineEnding(string lineEnding)
    {
    }

    public void WriteErrorLine(string message)
    {
    }

    public void WriteLine()
    {
    }

    public void WriteLine(string message)
    {
    }
}
