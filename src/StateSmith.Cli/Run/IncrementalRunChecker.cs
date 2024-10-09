using Spectre.Console;
using System;
using System.IO;

namespace StateSmith.Cli.Run;

public class IncrementalRunChecker
{
    private RunInfoStore _runInfoStore;
    private RunConsole _console;
    private string relativePath;
    private bool verbose;

    public IncrementalRunChecker(RunConsole console, string relativePath, bool verbose, RunInfoStore runInfoStore)
    {
        _console = console;
        this.relativePath = relativePath;
        this.verbose = verbose;
        _runInfoStore = runInfoStore;
    }

    public enum Result { OkToSkip, NeedsRunNoInfo, NeedsRunOutdated, NeedsRunMissingFiles, NeedsRunLastTimeFailed }

    private string GetRelativePath(string absolutePath)
    {
        return Path.GetRelativePath(relativePath, absolutePath);
    }

    public Result TestCsxFilePath(string csxAbsolutePath, bool rebuildIfLastFailure)
    {
        CsxRunInfo? runInfo;

        if (_runInfoStore == null || !_runInfoStore.csxRuns.TryGetValue(csxAbsolutePath, out runInfo))
        {
            _console.MarkupLine($"No previous run info found for {GetRelativePath(csxAbsolutePath)}. Code gen needed.");
            return Result.NeedsRunNoInfo;
        }

        return CheckRunInfo(runInfo, rebuildIfLastFailure);
    }

    public Result TestDiagramOnlyFilePath(string diagramAbsolutePath, bool rebuildIfLastFailure)
    {
        DiagramRunInfo? runInfo;

        if (_runInfoStore == null || !_runInfoStore.diagramRuns.TryGetValue(diagramAbsolutePath, out runInfo))
        {
            _console.MarkupLine($"No previous run info found for {GetRelativePath(diagramAbsolutePath)}. Code gen needed.");
            return Result.NeedsRunNoInfo;
        }

        return CheckRunInfo(runInfo, rebuildIfLastFailure);
    }

    private Result CheckRunInfo(RunInfo runInfo, bool rebuildIfLastFailure)
    {
        Result result = Result.OkToSkip;

        if (!runInfo.success && rebuildIfLastFailure)
        {
            result = Result.NeedsRunLastTimeFailed;
            return result;
        }

        foreach (var sourceFilePath in runInfo.GetSourceFileAbsolutePaths())
        {
            result = CheckSourceFile(sourceFilePath, runInfo);
            if (result != Result.OkToSkip)
                return result;
        }

        foreach (var outputFilePath in runInfo.writtenFileAbsolutePaths)
        {
            result = CheckOutputFile(outputFilePath, runInfo);
            if (result != Result.OkToSkip)
                return result;
        }

        return result;
    }

    private Result CheckSourceFile(string filePath, in RunInfo runInfo)
    {
        // if last run was a failure, we don't care if a source file is missing
        return CheckFile(filePath, runInfo.lastCodeGenStartDateTime, ignoreMissing: !runInfo.success);
    }

    private Result CheckOutputFile(string filePath, in RunInfo runInfo)
    {
        // output files are checked against the code gen end time because they are written after the source file
        // we do care if the output file is missing
        return CheckFile(filePath, runInfo.lastCodeGenEndDateTime, ignoreMissing: false);
    }

    private Result CheckFile(string filePath, in DateTime lastCodeGenDateTime, bool ignoreMissing)
    {
        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            if (ignoreMissing)
            {
                _console.QuietMarkupLine($"Expected file missing `{GetRelativePath(filePath)}`. Ignoring missing file.", filter: verbose);
            }
            else
            {
                _console.MarkupLine($"Expected file missing `{GetRelativePath(filePath)}`. Code gen needed.");
                return Result.NeedsRunMissingFiles;
            }
        }

        if (fileInfo.LastWriteTime >= lastCodeGenDateTime || fileInfo.CreationTime >= lastCodeGenDateTime)
        {
            _console.MarkupLine($"File `{GetRelativePath(filePath)}` [yellow]CHANGED[/] since last code gen. Code gen needed.");
            return Result.NeedsRunOutdated;
        }

        _console.QuietMarkupLine($"File `{GetRelativePath(filePath)}` hasn't changed since last code gen.", filter: verbose);

        return Result.OkToSkip;
    }
}
