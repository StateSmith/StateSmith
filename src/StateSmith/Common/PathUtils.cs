using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
