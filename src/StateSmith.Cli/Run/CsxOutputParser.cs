using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StateSmith.Cli.Run;

public class CsxOutputParser
{
    public List<string> GetPrintedDiagramPaths(string stateSmithConsoleOutput)
    {
        List<string> diagramPaths = new();

        // Parse: StateSmith Runner - Compiling file: `LightSm.drawio.svg`
        Regex diagramRegex = new(@"(?im)^\s*StateSmith Runner - Compiling file:\s*`([^`\r\n]+)`");

        var matches = diagramRegex.Matches(stateSmithConsoleOutput);

        foreach (Match match in matches.Cast<Match>())
        {
            diagramPaths.Add(match.Groups[1].Value);
        }

        if (diagramPaths.Count == 0)
        {
            throw new InvalidOperationException("Couldn't find diagram file from StateSmith console output: " + stateSmithConsoleOutput);
        }

        return diagramPaths;
    }

    public List<string> GetPrintedWrittenFilePaths(string stateSmithConsoleOutput)
    {
        List<string> writtenFilePaths = new();

        // Parse: StateSmith Runner - Writing to file `LightSm.js`
        Regex writtenFileRegex = new(@"(?im)^\s*StateSmith Runner - Writing to file.*?`([^`\r\n]+)`");

        var matches = writtenFileRegex.Matches(stateSmithConsoleOutput);

        foreach (Match match in matches.Cast<Match>())
        {
            writtenFilePaths.Add(match.Groups[1].Value);
        }

        if (writtenFilePaths.Count == 0)
        {
            throw new InvalidOperationException("Couldn't find written file from StateSmith console output: " + stateSmithConsoleOutput);
        }

        return writtenFilePaths;
    }

    /// <summary>
    /// <paramref name="csxRunInfo"/> must have its <see cref="CsxRunInfo.csxAbsolutePath"/> set beforehand.
    /// </summary>
    /// <param name="stateSmithConsoleOutput"></param>
    /// <param name="csxRunInfo"></param>
    public void ParseAndResolveFilePaths(string stateSmithConsoleOutput, CsxRunInfo csxRunInfo)
    {
        var diagramPaths = GetPrintedDiagramPaths(stateSmithConsoleOutput);
        foreach (var diagramPath in diagramPaths)
        {
            csxRunInfo.diagramAbsolutePaths.Add(ResolvePathFully(csxRunInfo, diagramPath));
        }

        var writtenFilePaths = GetPrintedWrittenFilePaths(stateSmithConsoleOutput);
        foreach (var writtenFilePath in writtenFilePaths)
        {
            csxRunInfo.writtenFileAbsolutePaths.Add(ResolvePathFully(csxRunInfo, writtenFilePath));
        }
    }

    private static string ResolvePathFully(CsxRunInfo csxRunInfo, string diagramPath)
    {
        // NOTE! By default, the printed diagram path is relative to the calling csx script file.
        // There are settings that can change this behavior however. Make it absolute or relative to another directory.
        if (!Path.IsPathFullyQualified(diagramPath))
        {
            diagramPath = Path.GetFullPath(Path.GetDirectoryName(csxRunInfo.csxAbsolutePath) + "/" + diagramPath);
        }

        return diagramPath;
    }
}
