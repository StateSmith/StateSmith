using Spectre.Console;
using System;
using System.IO;

namespace StateSmith.Cli.Run;

public class IncrementalRunChecker
{
    RunInfo? _readRunInfo;
    private IAnsiConsole _console;
    private string relativePath;
    public bool FoundMissingFile { get; private set; } = false;

    public IncrementalRunChecker(IAnsiConsole console, string relativePath)
    {
        _console = console;
        this.relativePath = relativePath;
    }

    public void SetReadRunInfo(RunInfo? readRunInfo)
    {
        _readRunInfo = readRunInfo;
    }

    public enum Result { OkToSkip, NeedsRunNoInfo, NeedsRunOutdated, NeedsRunMissingFiles }

    private string GetRelativePath(string absolutePath)
    {
        return Path.GetRelativePath(relativePath, absolutePath);
    }

    public Result TestFilePath(string csxAbsolutePath)
    {
        if (_readRunInfo == null || !_readRunInfo.csxRuns.ContainsKey(csxAbsolutePath))
        {
            ConsoleMarkupLine($"No previous run info found for {GetRelativePath(csxAbsolutePath)}. Code gen needed.");
            return Result.NeedsRunNoInfo;
        }

        Result result;

        var csxRun = _readRunInfo.csxRuns[csxAbsolutePath];
        result = CheckFile(csxAbsolutePath, csxRun.lastCodeGenStartDateTime);
        if (result != Result.OkToSkip)
            return result;

        foreach (var diagramPath in csxRun.diagramAbsolutePaths)
        {
            result = CheckFile(diagramPath, csxRun.lastCodeGenStartDateTime);
            if (result != Result.OkToSkip)
                return result;
        }

        foreach (var diagramPath in csxRun.writtenFileAbsolutePaths)
        {
            result = CheckFile(diagramPath, csxRun.lastCodeGenEndDateTime);
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

        QuietMarkupLine($"File `{GetRelativePath(filePath)}` hasn't changed since last code gen.");
        return Result.OkToSkip;
    }

    private void QuietMarkupLine(string message)
    {
        ConsoleMarkupLine($"[grey]{message}[/]");
    }

    private void ConsoleMarkupLine(string message)
    {
        _console.MarkupLine(message);
    }
}
