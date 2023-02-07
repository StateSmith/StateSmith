using StateSmith.Output.C99BalancedCoder1;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using System.Diagnostics;

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// Builds a single state machine and runs code generation.
/// </summary>
public class SmRunner : SmRunner.IExperimentalAccess
{
    public RunnerSettings Settings => settings;

    /// <summary>
    /// Dependency Injection Service Provider
    /// </summary>
    readonly DiServiceProvider diServiceProvider;

    readonly RunnerSettings settings;
    readonly InputSmBuilder inputSmBuilder;

    readonly ExceptionPrinter exceptionPrinter = new();
    readonly RenderConfigC renderConfigC;

    readonly string callingFilePath;

    public SmRunner(RunnerSettings settings, [System.Runtime.CompilerServices.CallerFilePath] string? callingFilePath = null)
    {
        this.callingFilePath = callingFilePath.ThrowIfNull();
        ResolveFilePaths(settings, callingFilePath);

        this.settings = settings;
        renderConfigC = new RenderConfigC();
        renderConfigC.SetFromIRenderConfigC(this.settings.renderConfig, settings.autoDeIndentAndTrimRenderConfigItems);

        diServiceProvider = DiServiceProvider.CreateDefault();

        diServiceProvider.AddConfiguration((services) =>
        {
            services.AddSingleton(settings.drawIoSettings);
            services.AddSingleton(settings.mangler);
            services.AddSingleton(settings.style);
            services.AddSingleton(renderConfigC);
            services.AddSingleton(new ExpansionConfigReaderObjectProvider(settings.renderConfig));
            services.AddSingleton(settings); // todo_low - split settings up more
            services.AddSingleton<IExpansionVarsPathProvider, CExpansionVarsPathProvider>();
        });

        inputSmBuilder = new(diServiceProvider);
    }

    /// <summary>
    /// A convenience constructor
    /// </summary>
    /// <param name="diagramPath">Relative to directory of script file that calls this constructor.</param>
    /// <param name="renderConfigC"></param>
    /// <param name="outputDirectory">Optional. If omitted, it will default to directory of <paramref name="diagramPath"/>. Relative to directory of script file that calls this constructor.</param>
    /// <param name="callingFilePath">Should normally be left unspecified so that C# can determine it automatically.</param>
    public SmRunner(string diagramPath, IRenderConfigC? renderConfigC = null, string? outputDirectory = null, [System.Runtime.CompilerServices.CallerFilePath] string? callingFilePath = null)
    : this(new RunnerSettings(renderConfigC ?? new DummyRenderConfigC(), diagramFile: diagramPath, outputDirectory: outputDirectory), callingFilePath: callingFilePath)
    {
    }

    /// <summary>
    /// Publicly exposed so that users can customize transformation behavior.
    /// </summary>
    public SmTransformer SmTransformer => inputSmBuilder.transformer;

    public void Run()
    {
        PrepareBeforeRun();

        try
        {
            Console.WriteLine();
            RunInputSmBuilder();
            RunCodeGen();
            OutputStageMessage("Finished normally.");
        }
        catch (Exception e)
        {
            if (settings.propagateExceptions)
            {
                throw;
            }

            Environment.ExitCode = -1; // lets calling process know that code gen failed

            exceptionPrinter.PrintException(e);
            MaybeDumpErrorDetailsToFile(e);
            OutputStageMessage("Finished with failure.");
        }

        Console.WriteLine();
    }

    internal void PrepareBeforeRun()
    {
        ResolveFilePaths(settings, callingFilePath);    // done again in case settings were modified after construction
    }

    private void RunInputSmBuilder()
    {
        OutputCompilingDiagramMessage();
        inputSmBuilder.ConvertDiagramFileToSmVertices(settings.diagramFile);
        FindStateMachine();
        inputSmBuilder.FinishRunning();
    }

    protected void RunCodeGen()
    {
        CodeGenRunner codeGenRunner = diServiceProvider.GetServiceOrCreateInstance();
        codeGenRunner.Run();
    }

    private void FindStateMachine()
    {
        if (settings.stateMachineName != null)
        {
            inputSmBuilder.FindStateMachineByName(settings.stateMachineName);
        }
        else
        {
            inputSmBuilder.FindSingleStateMachine();
        }

        OutputStageMessage($"State machine `{inputSmBuilder.GetStateMachine().Name}` selected.");
    }

    private void OutputCompilingDiagramMessage()
    {
        // https://github.com/StateSmith/StateSmith/issues/79
        string filePath = settings.diagramFile;
        if (settings.filePathPrintBase?.Trim().Length > 0)
        {
            filePath = Path.GetRelativePath(settings.filePathPrintBase, settings.diagramFile);
        }
        filePath = filePath.Replace('\\', '/');

        OutputStageMessage($"Compiling file: `{filePath}` "
            + ((settings.stateMachineName == null) ? "(no state machine name specified)" : $"with target state machine name: `{settings.stateMachineName}`")
            + "."
        );
    }

    protected static void OutputStageMessage(string message)
    {
        // todo_low add logger functionality
        Console.WriteLine("StateSmith Runner - " + message);
    }

    // https://github.com/StateSmith/StateSmith/issues/82
    private void MaybeDumpErrorDetailsToFile(Exception e)
    {
        if (!settings.dumpErrorsToFile)
        {
            Console.Error.WriteLine($"You can enable exception detail dumping by setting `{nameof(RunnerSettings)}.{nameof(RunnerSettings.dumpErrorsToFile)}` to true.");
            return;
        }

        var errorDetailFilePath = settings.diagramFile + ".err.txt";
        errorDetailFilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), errorDetailFilePath);
        exceptionPrinter.DumpExceptionDetails(e, errorDetailFilePath);
        Console.Error.WriteLine("Additional exception detail dumped to file: " + errorDetailFilePath);
    }

    private static void ResolveFilePaths(RunnerSettings settings, string? callingFilePath)
    {
        var relativeDirectory = Path.GetDirectoryName(callingFilePath).ThrowIfNull();
        settings.diagramFile = PathUtils.EnsurePathAbsolute(settings.diagramFile, relativeDirectory);
        settings.outputDirectory ??= Path.GetDirectoryName(settings.diagramFile).ThrowIfNull();
        settings.filePathPrintBase ??= relativeDirectory;

        settings.outputDirectory = ProcessDirPath(settings.outputDirectory, relativeDirectory);
        settings.filePathPrintBase = ProcessDirPath(settings.filePathPrintBase, relativeDirectory);
    }

    private static string ProcessDirPath(string dirPath, string relativeDirectory)
    {
        var resultPath = PathUtils.EnsurePathAbsolute(dirPath, relativeDirectory);
        resultPath = PathUtils.EnsureDirEndingSeperator(resultPath);
        return resultPath;
    }

    // ----------- experimental access  -------------

    public IExperimentalAccess GetExperimentalAccess() => this;
    DiServiceProvider IExperimentalAccess.DiServiceProvider => diServiceProvider;
    RunnerSettings IExperimentalAccess.Settings => settings;
    InputSmBuilder IExperimentalAccess.InputSmBuilder => inputSmBuilder;
    ExceptionPrinter IExperimentalAccess.ExceptionPrinter => exceptionPrinter;
    RenderConfigC IExperimentalAccess.RenderConfigC => renderConfigC;

    /// <summary>
    /// The API in this experimental access may brake often. It will eventually stabilize after enough use and feedback.
    /// </summary>
    public interface IExperimentalAccess
    {
        /// <summary>
        /// Dependency Injection Service Provider
        /// </summary>
        DiServiceProvider DiServiceProvider { get; }

        RunnerSettings Settings { get; }
        InputSmBuilder InputSmBuilder { get; }

        ExceptionPrinter ExceptionPrinter { get; }
        RenderConfigC RenderConfigC { get; }
    }
}


