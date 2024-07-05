using System;

#nullable enable

namespace StateSmith.Runner;

public class ConsolePrinter : IConsolePrinter
{
    string lineEnding = Environment.NewLine;

    public void SetLineEnding(string lineEnding)
    {
        this.lineEnding = lineEnding;
    }

    public void WriteErrorLine(string message) => Console.Error.WriteLine(ConvertLineEndingsIfNeeded(message));
    public void WriteLine(string message) => Console.WriteLine(ConvertLineEndingsIfNeeded(message));
    public void WriteLine() => Console.WriteLine();

    /// <summary>
    /// This is required for windows when this program is run from ss.cli which uses spectre console.
    /// On a windows machine using spectre console `Console.WriteLine($"a\nb\nc\n");` will output:
    /// <code>
    /// aaa
    ///    bbb
    ///       ccc
    /// </code>
    /// The \n is a literal newline, but needs the carriage return \r to go back to the start of the line.
    /// </summary>
    protected string ConvertLineEndingsIfNeeded(string message)
    {
        return message.ReplaceLineEndings(lineEnding);
    }
}
