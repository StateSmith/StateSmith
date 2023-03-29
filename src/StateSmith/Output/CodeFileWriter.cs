using StateSmith.Runner;
using System.IO;

namespace StateSmith.Output;

public class CodeFileWriter : ICodeFileWriter
{
    readonly IConsolePrinter consolePrinter;
    readonly FilePathPrinter pathPrinter;

    public CodeFileWriter(IConsolePrinter consolePrinter, FilePathPrinter pathPrinter)
    {
        this.consolePrinter = consolePrinter;
        this.pathPrinter = pathPrinter;
    }

    public void WriteFile(string filePath, string code)
    {
        consolePrinter.OutputStageMessage($"Writing to file `{pathPrinter.PrintPath(filePath)}`");
        File.WriteAllText(path:filePath, code);
    }
}

