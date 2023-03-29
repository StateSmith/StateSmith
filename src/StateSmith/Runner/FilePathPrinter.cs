#nullable enable

using System.IO;

namespace StateSmith.Runner;

public class FilePathPrinter
{
    /// <summary>
    /// Set to a blank string to output full absolute paths
    /// </summary>
    readonly string filePathPrintBase;

    /// <summary>
    /// </summary>
    /// <param name="filePathPrintBase">Set to a blank string to output full absolute paths</param>
    public FilePathPrinter(string filePathPrintBase)
    {
        this.filePathPrintBase = filePathPrintBase;
    }

    public string PrintPath(string filePath)
    {
        if (filePathPrintBase.Trim().Length > 0)
        {
            filePath = Path.GetRelativePath(filePathPrintBase, filePath);
        }
        filePath = filePath.Replace('\\', '/');
        return filePath;
    }
}

