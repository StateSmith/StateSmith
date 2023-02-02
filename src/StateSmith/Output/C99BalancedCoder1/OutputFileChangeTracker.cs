#nullable enable

namespace StateSmith.Output.C99BalancedCoder1;

public class OutputFileChangeTracker
{
    private OutputFile outputFile;
    private int lastStringBufferLength;

    public OutputFileChangeTracker(OutputFile outputFile)
    {
        this.outputFile = outputFile;
        Reset();
    }

    public bool TestChanged()
    {
        return outputFile.GetStringBufferLength() != lastStringBufferLength;
    }

    public void Reset()
    {
        lastStringBufferLength = outputFile.GetStringBufferLength();
    }

    public bool PopChanged()
    {
        bool changed = TestChanged();
        Reset();
        return changed;
    }

    public void OutputNewLineOnChange()
    {
        if (PopChanged())
        {
            outputFile.AppendLine();
        }
    }
}
