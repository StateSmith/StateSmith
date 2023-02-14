using System;

#nullable enable

namespace StateSmith.Runner;

public class ConsolePrinter : IConsolePrinter
{
    public void WriteErrorLine(string message) => Console.Error.WriteLine(message);
    public void WriteLine(string message) => Console.WriteLine(message);
    public void WriteLine() => Console.WriteLine();
}
