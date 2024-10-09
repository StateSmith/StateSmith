using Spectre.Console;
using System;
using System.IO;

namespace StateSmith.Cli.Run;

public class IncrementalRunChecker
{
    RunInfoStore _runInfoStore;
    private RunConsole _console;
    private string relativePath;
    public bool FoundMissingFile { get; private set; } = false;
    bool verbose;

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
            ConsoleMarkupLine($"No previous run info found for {GetRelativePath(csxAbsolutePath)}. Code gen needed.");
            return Result.NeedsRunNoInfo;
        }

        Result result;

        if (!runInfo.success && rebuildIfLastFailure)
        {
            result = Result.NeedsRunLastTimeFailed;
            return result;
        }

        result = CheckFile(csxAbsolutePath, runInfo.lastCodeGenStartDateTime);
        if (result != Result.OkToSkip)
            return result;

        foreach (var diagramPath in runInfo.diagramAbsolutePaths)
        {
            result = CheckFile(diagramPath, runInfo.lastCodeGenStartDateTime);
            if (result != Result.OkToSkip)
                return result;
        }

        foreach (var outputFilePath in runInfo.writtenFileAbsolutePaths)
        {
            // output files are checked against the end time because they are written after the source file
            result = CheckFile(outputFilePath, runInfo.lastCodeGenEndDateTime);
            if (result != Result.OkToSkip)
                return result;
        }

        return result;
    }

    public Result TestDiagramOnlyFilePath(string diagramAbsolutePath, bool rebuildIfLastFailure)
    {
        DiagramRunInfo? runInfo;

        if (_runInfoStore == null || !_runInfoStore.diagramRuns.TryGetValue(diagramAbsolutePath, out runInfo))
        {
            ConsoleMarkupLine($"No previous run info found for {GetRelativePath(diagramAbsolutePath)}. Code gen needed.");
            return Result.NeedsRunNoInfo;
        }

        Result result;

        if (!runInfo.success && rebuildIfLastFailure)
        {
            result = Result.NeedsRunLastTimeFailed;
            return result;
        }

        result = CheckFile(diagramAbsolutePath, runInfo.lastCodeGenStartDateTime);
        if (result != Result.OkToSkip)
            return result;

        foreach (var outputFilePath in runInfo.writtenFileAbsolutePaths)
        {
            // output files are checked against the end time because they are written after the source file
            result = CheckFile(outputFilePath, runInfo.lastCodeGenEndDateTime);
            if (result != Result.OkToSkip)
                return result;
        }

        return result;
    }

    private Result CheckFile(string filePath, in DateTime lastCodeGenDateTime)
    {
        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            FoundMissingFile = true;
            ConsoleMarkupLine($"Expected file missing `{GetRelativePath(filePath)}`. Code gen needed.");
            return Result.NeedsRunMissingFiles;
        }

        if (fileInfo.LastWriteTime >= lastCodeGenDateTime)
        {
            ConsoleMarkupLine($"File `{GetRelativePath(filePath)}` [yellow]CHANGED[/] since last code gen. Code gen needed.");
            return Result.NeedsRunOutdated;
        }

        if (verbose)
        {
            QuietMarkupLine($"File `{GetRelativePath(filePath)}` hasn't changed since last code gen.");
        }
        return Result.OkToSkip;
    }

    // TODOLOW use run console that does this already
    private void QuietMarkupLine(string message)
    {
        ConsoleMarkupLine($"[grey]{message}[/]");
    }

    private void ConsoleMarkupLine(string message)
    {
        _console.MarkupLine(message);
    }
}
