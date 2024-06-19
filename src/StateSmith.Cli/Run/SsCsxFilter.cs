using System.IO;
using System.Text.RegularExpressions;

namespace StateSmith.Cli.Run;

/// <summary>
/// Determines if a file is a StateSmith csx script file.
/// </summary>
public class SsCsxFilter
{
    public static bool IsScriptFile(string path)
    {
        return Path.GetExtension(path) == ".csx";
    }

    public bool IsTargetScriptFile(string path, string fileText)
    {
        if (IsScriptFile(path))
        {
            return IsTargetScriptContent(fileText);
        }

        return false;
    }

    public static bool IsTargetScriptContent(string scriptCodeText)
    {
        if (Regex.IsMatch(scriptCodeText, @"(?xim)
            ^ \s*
            [#]r \s* ""nuget \s*  : \s* StateSmith \s* ,"))
        {
            return true;
        }

        return false;
    }
}
