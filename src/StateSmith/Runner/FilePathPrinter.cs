#nullable enable

using System;
using System.IO;

namespace StateSmith.Runner;

public class FilePathPrinter
{
    /// <summary>
    /// Set to a blank string to output full absolute paths
    /// </summary>
    readonly Func<string> filePathPrintBaseProvider;

    /// <summary>
    /// </summary>
    /// <param name="filePathPrintBaseProvider">Set to a blank string to output full absolute paths</param>
    public FilePathPrinter(Func<string> filePathPrintBaseProvider)
    {
        this.filePathPrintBaseProvider = filePathPrintBaseProvider;
    }

    public string PrintPath(string filePath)
    {
        var filePathPrintBase = filePathPrintBaseProvider();
        if (filePathPrintBase.Trim().Length > 0)
        {
            filePath = Path.GetRelativePath(filePathPrintBase, filePath);
        }
        filePath = filePath.Replace('\\', '/');
        return filePath;
    }
}

