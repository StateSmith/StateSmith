using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using StateSmith.Runner;

namespace StateSmith.Cli.Run;

/// <summary>
/// Finds StateSmith .csx files, and also StateSmith diagrams.
/// </summary>
public class SsCsxDiagramFileFinder
{
    const string InlineIgnoreThisFileToken = "statesmith.cli-ignore-this-file";

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
        ScanResults scanResults = new();

        PatternMatchingResult result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(searchDirectory)));

        SsDiagramFilter diagramFilter = new(new());   // todolow - get diagram filter from constructor
        SsCsxFilter ssCsxFilter = new();

        foreach (var file in result.Files)
        {
            string fullPath = searchDirectory + "/" + file.Path;

            // we need to inspect file contents for rest of checks
            var fileText = File.ReadAllText(fullPath);

            if (fileText.Contains(SsCsxDiagramFileFinder.InlineIgnoreThisFileToken))
            {
                scanResults.ignoredFiles.Add(file.Path);
                continue;
            }

            if (ssCsxFilter.IsTargetScriptFile(fullPath, fileText: fileText))
            {
                scanResults.targetCsxFiles.Add(file.Path);
            }
            else
            {
                // check for broken svg files
                if (IsBrokenDrawioSvgFile(fullPath: fullPath, fileText: fileText))
                {
                    scanResults.brokenDrawioSvgFiles.Add(file.Path);
                }
                else if (diagramFilter.IsTargetDiagramFile(fullPath, fileText: fileText))
                {
                    scanResults.targetDiagramFiles.Add(file.Path);
                }
                else
                {
                    scanResults.nonMatchingFiles.Add(file.Path);
                }
            }
        }

        return scanResults;
    }

    /// <summary>
    /// https://github.com/StateSmith/StateSmith/issues/341
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="fileText"></param>
    /// <returns></returns>
    public static bool IsBrokenDrawioSvgFile(string fullPath, string fileText)
    {
        if (!DiagramFileAssociator.IsDrawIoSvgFile(fullPath))
        {
            return false;
        }

        // See https://github.com/StateSmith/StateSmith/issues/341 for detection strategy

        bool containsGOpeningTag = fileText.Contains("<g>"); // normal files have this
        bool containsGEmptyTag = fileText.Contains("<g/>");  // broken files have this

        if (containsGOpeningTag && !containsGEmptyTag)
        {
            // normal file detected
            return false;
        }

        // This is either an empty file or a broken file
        // Empty files have width and height of 1px
        bool isEmptyFile = fileText.Contains("width=\"1px\"") && fileText.Contains("height=\"1px\"");

        return !isEmptyFile;
    }

    public class ScanResults
    {
        /// <summary>
        /// relative to searchDirectory
        /// </summary>
        public List<string> targetCsxFiles = new();

        /// <summary>
        /// relative to searchDirectory
        /// </summary>
        public List<string> ignoredFiles = new();

        /// <summary>
        /// relative to searchDirectory
        /// </summary>
        public List<string> targetDiagramFiles = new();

        /// <summary>
        /// relative to searchDirectory
        /// </summary>
        public List<string> nonMatchingFiles = new();

        /// <summary>
        /// relative to searchDirectory
        /// https://github.com/StateSmith/StateSmith/issues/341
        /// </summary>
        public List<string> brokenDrawioSvgFiles = new();
    }
}

