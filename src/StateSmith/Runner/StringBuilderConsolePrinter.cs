using System.Text;

namespace StateSmith.Runner;

public class StringBuilderConsolePrinter : IConsolePrinter
{
    public StringBuilder sb = new();

    public void SetLineEnding(string lineEnding)
    {
        // just ignore this
    }

    void IConsolePrinter.WriteErrorLine(string message) => sb.AppendLine(message);
    void IConsolePrinter.WriteLine(string message) => sb.AppendLine(message);
    void IConsolePrinter.WriteLine() => sb.AppendLine();
}
