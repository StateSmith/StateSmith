using StateSmith.Runner;
using System.Text;

namespace StateSmithTest;

public class StringBufferConsolePrinter : IConsolePrinter
{
    public readonly StringBuilder sb = new();

    void IConsolePrinter.WriteErrorLine(string message) => sb.AppendLine(message);
    void IConsolePrinter.WriteLine(string message) => sb.AppendLine(message);
    void IConsolePrinter.WriteLine() => sb.AppendLine();
}
