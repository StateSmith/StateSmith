using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Spectre.Console;

namespace StateSmith.Cli.Run;

/// <summary>
/// Finds StateSmith .csx files, and also StateSmith diagrams.
/// </summary>
public class SsCsxDiagramFileFinder
{
    Matcher matcher;
    private bool hasIncludePatterns;

    public SsCsxDiagramFileFinder()
    {
        matcher = new();
    }

    public void SetAsRecursive()
    {
        foreach (var ext in StandardFiles.GetStandardFileExtensions())
        {
            matcher.AddInclude($"./**/*{ext}");
        }
    }

    public void ClearIncludePatterns()
    {
        matcher = new(); // Reset the matcher
    }

    public void AddDefaultIncludePatternIfNone()
    {
        if (!hasIncludePatterns)
        {
            foreach (var ext in StandardFiles.GetStandardFileExtensions())
            {
                matcher.AddInclude($"./*{ext}");
            }
        }
    }

    public void AddIncludePattern(string pathPattern)
    {
        hasIncludePatterns = true;
        matcher.AddInclude(pathPattern);
    }

    public void AddIncludePatterns(IEnumerable<string>? pathPatterns)
    {
        if (pathPatterns == null)
            return;

        foreach (var pattern in pathPatterns)
        {
            AddIncludePattern(pattern);
        }
    }

    public void AddExcludePattern(string pathPattern)
    {
        matcher.AddExclude(pathPattern);
    }

    public void AddExcludePatterns(IEnumerable<string>? pathPatterns)
    {
        if (pathPatterns == null)
            return;

        foreach (var pattern in pathPatterns)
        {
            AddExcludePattern(pattern);
        }
    }

    public ScanResults Scan(string searchDirectory)
    {
        List<string> targetCsxFiles = new();
        List<string> targetDiagramFiles = new();

        PatternMatchingResult result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(searchDirectory)));

        SsDiagramFilter diagramFilter = new(new());   // todolow - get diagram filter from constructor
        SsCsxFilter ssCsxFilter = new();

        foreach (var file in result.Files)
        {
            string fullPath = searchDirectory + "/" + file.Path;

            // we need to inspect file contents for rest of checks
            var fileText = File.ReadAllText(fullPath);

            if (ssCsxFilter.IsTargetScriptFile(fullPath, fileText: fileText))
            {
                targetCsxFiles.Add(file.Path);
            }
            else if (diagramFilter.IsTargetDiagramFile(fullPath, fileText: fileText))
            {
                targetDiagramFiles.Add(file.Path);
            }
        }

        return new ScanResults(targetCsxFiles, diagramFiles: targetDiagramFiles);
    }

    public class ScanResults
    {
        public List<string> targetCsxFiles;
        public List<string> diagramFiles;

        public ScanResults(List<string> targetCsxFiles, List<string> diagramFiles)
        {
            this.targetCsxFiles = targetCsxFiles;
            this.diagramFiles = diagramFiles;
        }
    }
}

