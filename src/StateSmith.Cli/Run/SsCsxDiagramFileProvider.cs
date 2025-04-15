using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Spectre.Console;
using StateSmith.Runner;

namespace StateSmith.Cli.Run;

/// <summary>
/// Finds StateSmith .csx files, and also StateSmith diagrams.
/// </summary>
public class SsCsxDiagramFileProvider
{
    const string InlineIgnoreThisFileToken = "statesmith.cli-ignore-this-file";

    Matcher matcher;
    private bool hasIncludePatterns;
    private IList<string> specificFilePaths = new List<string>();

    public SsCsxDiagramFileProvider()
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

    /// <summary>
    /// Will provide files if explicitly set, otherwise will search for files.
    /// </summary>
    /// <param name="searchDirectory"></param>
    /// <returns></returns>
    public FileResults GetOrFindFiles(string searchDirectory)
    {
        FileResults fileResults = new(searchDirectory);

        if (specificFilePaths.Count > 0)
        {
            GetSpecificFileResults(specificFilePaths, fileResults);
        }
        else
        {
            ScanForFiles(fileResults);
        }

        return fileResults;
    }

    private void ScanForFiles(FileResults scanResults)
    {
        string searchDirectory = scanResults.searchDirectory;
        PatternMatchingResult result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(searchDirectory)));

        SsDiagramFilter diagramFilter = new();   // todolow - get diagram filter from constructor
        SsCsxFilter ssCsxFilter = new();

        foreach (var file in result.Files)
        {
            string fullPath = searchDirectory + "/" + file.Path;

            // we need to inspect file contents for rest of checks
            var fileText = File.ReadAllText(fullPath);

            if (fileText.Contains(InlineIgnoreThisFileToken))
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
    }

    private static void GetSpecificFileResults(IList<string> filePaths, FileResults fileResults)
    {
        string searchDirectory = fileResults.searchDirectory;

        foreach (var filePath in filePaths)
        {
            // we may have empty strings in the list if user mixed files with options
            if (string.IsNullOrWhiteSpace(filePath))
            {
                continue;
            }

            string relativePath = filePath;

            // scan results are relative to searchDirectory, so we need to make sure file path is relative
            if (Path.IsPathRooted(relativePath))
            {
                relativePath = Path.GetRelativePath(searchDirectory, filePath);
            }

            if (SsCsxFilter.IsScriptFile(relativePath))
            {
                fileResults.targetCsxFiles.Add(relativePath);
            }
            else
            {
                fileResults.targetDiagramFiles.Add(relativePath);
                // don't worry about checking if diagram type is supported here. That happens later.
            }
        }
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

    public void SetSpecificFiles(IList<string> readonlyFilePaths, string searchDirectory)
    {
        // remove empty file path strings
        var paths = readonlyFilePaths.Where(filePath => !string.IsNullOrWhiteSpace(filePath)).ToList();

        // windows shells don't typically expand globs (without super verbose syntax), so we need to do it ourselves
        bool needsGlobbing = false;
        foreach (var path in paths)
        {
            if (path.Contains('*'))
            {
                needsGlobbing = true;
                matcher.AddInclude(path);
            }
            else
            {
                specificFilePaths.Add(path);
            }
        }

        if (needsGlobbing)
        {
            PatternMatchingResult result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(searchDirectory)));

            foreach (var file in result.Files)
            {
                specificFilePaths.Add(file.Path);
            }
        }
    }

    public class FileResults
    {
        public string searchDirectory;

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

        public List<string> AbsoluteCsxFiles => targetCsxFiles.Select(relPath => Path.Combine(searchDirectory, relPath)).ToList();
        public List<string> AbsoluteDiagramFiles => targetDiagramFiles.Select(relPath => Path.Combine(searchDirectory, relPath)).ToList();

        public bool HasTargetFiles => targetCsxFiles.Count > 0 || targetDiagramFiles.Count > 0;

        public FileResults(string searchDirectory)
        {
            this.searchDirectory = searchDirectory;
        }
    }
}

