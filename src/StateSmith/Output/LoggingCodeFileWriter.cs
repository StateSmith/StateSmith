using StateSmith.Runner;
using System.Collections.Generic;

namespace StateSmith.Output;

/// <summary>
/// This is used to log the file paths that code files are written to.
/// </summary>
public class LoggingCodeFileWriter : CodeFileWriter
{
    public List<string> filePathsWritten = new();

    public LoggingCodeFileWriter(IConsolePrinter consolePrinter, FilePathPrinter pathPrinter) : base(consolePrinter, pathPrinter)
    {
    }

    public override void WriteFile(string filePath, string code)
    {
        filePathsWritten.Add(filePath);
        base.WriteFile(filePath, code);
    }
}
