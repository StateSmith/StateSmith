#nullable enable

using System.IO;

namespace StateSmith.Common;

public class PathUtils
{
    public static string EnsurePathAbsolute(string path, string defaultDirectory)
    {
        string resultPath;

        defaultDirectory = EnsureDirEndingSeperator(defaultDirectory);

        if (Path.IsPathFullyQualified(path))
            resultPath = path;
        else
            resultPath = defaultDirectory + path;

        return resultPath;
    }

    public static string EnsureDirEndingSeperator(string path)
    {
        string resultPath = path;

        if (!Path.EndsInDirectorySeparator(resultPath))
            resultPath += Path.DirectorySeparatorChar;

        return resultPath;
    }

    public static string GetThisDir([System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        return Path.GetDirectoryName(callerFilePath) + "/";
    }
}
