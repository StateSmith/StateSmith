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
        Regex r = new Regex(@"(?im)^\s*StateSmith Runner - Compiling file:\s*`([^`]+)`");

        var matches = r.Matches(stateSmithConsoleOutput);

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

    /// <summary>
    /// <paramref name="csxRunInfo"/> must have its <see cref="CsxRunInfo.csxAbsolutePath"/> set beforehand.
    /// </summary>
    /// <param name="stateSmithConsoleOutput"></param>
    /// <param name="csxRunInfo"></param>
    public void ParseAndResolveFilePaths(string stateSmithConsoleOutput, CsxRunInfo csxRunInfo)
    {
        var diagramPaths = GetPrintedDiagramPaths(stateSmithConsoleOutput);

        for (int i = 0; i < diagramPaths.Count; i++)
        {
            var diagramPath = diagramPaths[i];

            // NOTE! By default, the printed diagram path is relative to the calling csx script file.
            // There are settings that can change this behavior however. Make it absolute or relative to another directory.
            if (!Path.IsPathFullyQualified(diagramPath))
            {
                diagramPath = Path.GetFullPath(Path.GetDirectoryName(csxRunInfo.csxAbsolutePath) + "/" + diagramPath);
            }
            
            csxRunInfo.diagramAbsolutePaths.Add(diagramPath);
        }
    }
}
