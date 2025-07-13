#nullable enable

using Microsoft.Extensions.DependencyInjection;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using System;
using StateSmith.SmGraph;
using StateSmith.Output.Sim;
using System.IO;

namespace StateSmith.Runner;

/// <summary>
/// Builds a single state machine and runs code generation.
/// </summary>
public class SmRunner
{    
    /// <summary>
    /// Convenience method to create a new instance of SmRunner without constructing a separate RunnerSettings object.
    /// </summary>
    /// <param name="diagramPath">Relative to directory of script file that calls this constructor.</param>
    /// <param name="renderConfig"></param>
    /// <param name="outputDirectory">Optional. If omitted, it will default to directory of <paramref name="diagramPath"/>. Relative to directory of script file that calls this constructor.</param>
    /// <param name="algorithmId">Optional. Will allow you to choose which algorithm to use when multiple are supported. Ignored if custom code generator used.</param>
    /// <param name="transpilerId">Optional. Defaults to C99. Allows you to specify which programming language to generate for. Ignored if custom code generator used.</param>
    /// <param name="serviceProvider">Optional. Provides a way to override registered dependency injection services.</param>
    /// <param name="callingFilePath">Should normally be left unspecified so that C# can determine it automatically.</param>
    /// <returns></returns>
    public static SmRunner Create(string diagramPath,
        IRenderConfig? renderConfig = null,
        string? outputDirectory = null,
        AlgorithmId algorithmId = AlgorithmId.Default,
        TranspilerId transpilerId = TranspilerId.Default,
        IServiceProvider? serviceProvider = null,
        [System.Runtime.CompilerServices.CallerFilePath] string? callingFilePath = null)
    {
        return Create(new RunnerSettings(diagramFile: diagramPath, outputDirectory: outputDirectory, algorithmId: algorithmId, transpilerId: transpilerId), renderConfig, serviceProvider, callerFilePath: callingFilePath);
    }

    /// <summary>
    /// Factory method to create a new SmRunner instance.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="renderConfig"></param>
    /// <param name="serviceProvider">Optional. Provides a way to override registered dependency injection services.</param>
    /// <param name="callerFilePath">Should normally be left unspecified so that C# can determine it automatically.</param>
    /// <returns></returns>
    public static SmRunner Create(RunnerSettings settings, IRenderConfig? renderConfig, IServiceProvider? serviceProvider = null, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        var sp = serviceProvider ?? RunnerServiceProviderFactory.CreateDefault();

        // Set the context for the SmRunner
        var context = sp.GetRequiredService<RunnerContext>();
        context.runnerSettings = settings;
        if (renderConfig != null)
        {
            context.renderConfig = renderConfig;
        }
        context.callerFilePath = callerFilePath.ThrowIfNull();

        return sp.GetRequiredService<SmRunner>();
    }


    // TODO remove
    public RunnerSettings Settings => context.runnerSettings;


    /// <summary>
    /// The context that holds the dynamic configuration (settings, renderconfig) for this run of the runner.
    /// </summary>
    private readonly RunnerContext context;

    private readonly InputSmBuilder inputSmBuilder;
    private readonly SmTransformer transformer;
    private readonly ExceptionPrinter exceptionPrinter;
    private readonly IConsolePrinter consolePrinter;
    private readonly Func<SimWebGenerator> simWebGeneratorProvider;
    private readonly AlgoTranspilerCustomizer algoTranspilerCustomizer;
    private readonly SmDesignDescriber smDesignDescriber;
    private readonly OutputInfo outputInfo;
    private readonly FilePathPrinter filePathPrinter;
    private readonly Func<ICodeGenRunner> codeGenRunnerProvider;
    private readonly Func<PreDiagramSettingsReader?> preDiagramSettingsReaderProvider;


    /// <summary>
    /// Constructor. Mostly intended to be used by DI.
    /// </summary>
    /// <param name="context">This context object stores the runtime configuration for a given run </param>
    /// <param name="inputSmBuilder">The builder that will convert the input diagram into a StateMachine vertex.</param>
    /// <param name="exceptionPrinter">Used to print exceptions.</param>
    /// <param name="consolePrinter">Used to print messages to the console.</param>
    /// <param name="simWebGeneratorProvider">A function that provides a SimWebGenerator instance.</param>
    /// <param name="algoTranspilerCustomizer">Allows customization of the algorithm and transpiler settings.</param>
    /// <param name="smDesignDescriber">Describes the state machine design.</param>
    /// <param name="outputInfo">Holds information about the output, such as file paths and names.</param>
    /// <param name="filePathPrinter">Used to print file paths in a consistent manner.</param>
    /// <param name="codeGenRunnerProvider">A function that provides an ICodeGenRunner instance.</param>
    /// <param name="transformer">Instance of SmTransformer</param>
    /// <param name="preDiagramSettingsReaderProvider">Reads settings from the diagram</param>
    [Obsolete("This constructor is meant for internal use only. Use SmRunner.Create() instead.")]
    public SmRunner(RunnerContext context, InputSmBuilder inputSmBuilder, ExceptionPrinter exceptionPrinter, IConsolePrinter consolePrinter, Func<SimWebGenerator> simWebGeneratorProvider, AlgoTranspilerCustomizer algoTranspilerCustomizer, SmDesignDescriber smDesignDescriber, OutputInfo outputInfo, FilePathPrinter filePathPrinter, Func<ICodeGenRunner> codeGenRunnerProvider, SmTransformer transformer, Func<PreDiagramSettingsReader?> preDiagramSettingsReaderProvider)
    {
        this.context = context;
        this.inputSmBuilder = inputSmBuilder;
        this.exceptionPrinter = exceptionPrinter;
        this.consolePrinter = consolePrinter;
        this.simWebGeneratorProvider = simWebGeneratorProvider;
        this.algoTranspilerCustomizer = algoTranspilerCustomizer;
        this.smDesignDescriber = smDesignDescriber;
        this.outputInfo = outputInfo;
        this.filePathPrinter = filePathPrinter;
        this.codeGenRunnerProvider = codeGenRunnerProvider;
        this.transformer = transformer;
        this.preDiagramSettingsReaderProvider = preDiagramSettingsReaderProvider;

        ResolveFilePaths(context.runnerSettings, context.callerFilePath);
        ReadDiagramRenderConfigs();
    }

    /// <summary>
    /// Legacy constructor provided for backward compatibility with older CSX scripts
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="renderConfig"></param>
    /// <param name="callerFilePath">Don't provide this argument. C# will automatically populate it.</param>
    /// <param name="serviceProvider">Optional dependency injection service provider, for overrides</param>
    [Obsolete("This constructor is intended for use by legacy CSX scripts. Use SmRunner.Create() instead.")]   
    public SmRunner(RunnerSettings settings, IRenderConfig? renderConfig = null, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null, IServiceProvider? serviceProvider = null)
    {
        var serviceProvider = serviceProvider ?? RunnerServiceProviderFactory.CreateDefault();
        this.context = this.serviceProvider.GetRequiredService<RunnerContext>();
        this.context.runnerSettings = settings;
        this.context.renderConfig = renderConfig ?? new DummyIRenderConfig();
        this.context.callerFilePath = callerFilePath.ThrowIfNull();
        ResolveFilePaths(settings, this.context.callerFilePath);
        ReadDiagramRenderConfigs();
    }

    /// <summary>
    /// Legacy constructor provided for backward compatibility with older CSX scripts
    /// </summary>
    /// <param name="diagramPath">Relative to directory of script file that calls this constructor.</param>
    /// <param name="renderConfig"></param>
    /// <param name="outputDirectory">Optional. If omitted, it will default to directory of <paramref name="diagramPath"/>. Relative to directory of script file that calls this constructor.</param>
    /// <param name="algorithmId">Optional. Will allow you to choose which algorithm to use when multiple are supported. Ignored if custom code generator used.</param>
    /// <param name="transpilerId">Optional. Defaults to C99. Allows you to specify which programming language to generate for. Ignored if custom code generator used.</param>
    /// <param name="callerFilePath">Should normally be left unspecified so that C# can determine it automatically.</param>
    /// <param name="serviceProvider">Optional dependency injection service provider, for overrides</param>
    [Obsolete("This constructor is intended for use by legacy CSX scripts. Use SmRunner.Create() instead.")]   
    public SmRunner(string diagramPath,
        IRenderConfig? renderConfig = null,
        string? outputDirectory = null,
        AlgorithmId algorithmId = AlgorithmId.Default,
        TranspilerId transpilerId = TranspilerId.Default,
        [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null,
        IServiceProvider? serviceProvider = null)
#pragma warning disable CS0618 // Type or member is obsolete
    : this(new RunnerSettings(diagramFile: diagramPath, outputDirectory: outputDirectory, algorithmId: algorithmId, transpilerId: transpilerId), renderConfig, callerFilePath: callerFilePath, serviceProvider: serviceProvider)
#pragma warning restore CS0618 // Type or member is obsolete
    {
    }

    /// <summary>
    /// Runs StateSmith.
    /// </summary>
    public void Run()
    {
        PrepareBeforeRun();

        algoTranspilerCustomizer.Customize(context.runnerSettings.algorithmId, context.runnerSettings.transpilerId);

        if (context.runnerSettings.transpilerId == TranspilerId.NotYetSet)
            throw new ArgumentException("TranspilerId must be set before running code generation");

        Exception? exception = null; // TODO what is this for? This exception handling logic seems quite complicated
        try
        {
            if (preDiagramSettingsReaderProvider() != null)
            {
                // we need to prevent diagram settings from being applied twice
                DiagramBasedSettingsPreventer.Process(inputSmBuilder.transformer);
            }

            consolePrinter.WriteLine();
            consolePrinter.WriteLine("StateSmith lib ver - " + LibVersionInfo.GetVersionInfoString());
            OutputCompilingDiagramMessage();

            var sm = SetupAndFindStateMachine(inputSmBuilder, context.runnerSettings);
            outputInfo.baseFileName = sm.Name;

            consolePrinter.OutputStageMessage($"State machine `{sm.Name}` selected.");
            smDesignDescriber.Prepare();
            smDesignDescriber.DescribeBeforeTransformations();

            inputSmBuilder.FinishRunning();
            smDesignDescriber.DescribeAfterTransformations();
            codeGenRunnerProvider().Run();

            if (context.runnerSettings.simulation.enableGeneration)
            {
                var simWebGenerator = simWebGeneratorProvider();
                simWebGenerator.RunnerSettings.propagateExceptions = context.runnerSettings.propagateExceptions;
                simWebGenerator.RunnerSettings.outputStateSmithVersionInfo = context.runnerSettings.outputStateSmithVersionInfo;
                simWebGenerator.Generate(diagramPath: context.runnerSettings.DiagramPath, outputDir: context.runnerSettings.simulation.outputDirectory.ThrowIfNull());
            }

            consolePrinter.OutputStageMessage("Finished normally.");
        }
        catch (Exception e)
        {
            exception = e;

            // print error detail before rethrowing https://github.com/StateSmith/StateSmith/issues/375
            OutputExceptionDetail(e);

            if (context.runnerSettings.propagateExceptions)
            {
                throw;
            }
        }

        consolePrinter.WriteLine();
        if (exception != null)
        {
            throw new FinishedWithFailureException();
        }
    
    }

    // ------------ private methods ----------------


    /// <summary>
    /// Outputs a message about the diagram file being compiled.
    /// </summary>
    private void OutputCompilingDiagramMessage()
    {

        string filePath = context.runnerSettings.DiagramPath;
        filePath = filePathPrinter.PrintPath(filePath);

        consolePrinter.OutputStageMessage($"Compiling file: `{filePath}` "
            + ((context.runnerSettings.stateMachineName == null) ? "(no state machine name specified)" : $"with target state machine name: `{context.runnerSettings.stateMachineName}`")
            + "."
        );
    }

    /// <summary>
    /// Finds and returns the state machine from the input builder, using settings.
    /// </summary>
    /// // TODO make InputSmBuilder initialize itself rather than have SmRunner do it
    private static StateMachine SetupAndFindStateMachine(InputSmBuilder inputSmBuilder, RunnerSettings settings)
    {
        // If the inputSmBuilder already has a state machine, then use it.
        // Used by test code.
        // Might also be used in future to allow compiling plantuml without a diagram file.
        if (inputSmBuilder.HasStateMachine)
        {
            return inputSmBuilder.GetStateMachine();
        }

        inputSmBuilder.ConvertDiagramFileToSmVertices(settings.DiagramPath);

        if (settings.stateMachineName != null)
        {
            inputSmBuilder.FindStateMachineByName(settings.stateMachineName);
        }
        else
        {
            inputSmBuilder.FindSingleStateMachine();
        }

        return inputSmBuilder.GetStateMachine();
    }

    // TODO the callingFilePath introduces additional complexity, I think just to support CSX scripts.
    // Would it be almost as good to just use the path to the diagram file?
    private static void ResolveFilePaths(RunnerSettings settings, string? callingFilePath)
    {
        var relativeDirectory = Path.GetDirectoryName(callingFilePath).ThrowIfNull();
        settings.DiagramPath = PathUtils.EnsurePathAbsolute(settings.DiagramPath, relativeDirectory);

        settings.outputDirectory ??= Path.GetDirectoryName(settings.DiagramPath).ThrowIfNull();
        settings.outputDirectory = ProcessDirPath(settings.outputDirectory, relativeDirectory);

        settings.filePathPrintBase ??= relativeDirectory;
        settings.filePathPrintBase = ProcessDirPath(settings.filePathPrintBase, relativeDirectory);

        settings.smDesignDescriber.outputDirectory ??= settings.outputDirectory;
        settings.smDesignDescriber.outputDirectory = ProcessDirPath(settings.smDesignDescriber.outputDirectory, relativeDirectory);

        if (settings.simulation.enableGeneration)
        {
            settings.simulation.outputDirectory ??= settings.outputDirectory;
            settings.simulation.outputDirectory = ProcessDirPath(settings.simulation.outputDirectory, relativeDirectory);
        }
    }

    private static string ProcessDirPath(string dirPath, string relativeDirectory)
    {
        var resultPath = PathUtils.EnsurePathAbsolute(dirPath, relativeDirectory);
        resultPath = PathUtils.EnsureDirEndingSeperator(resultPath);
        return resultPath;
    }

    /// <summary>
    /// Force application number parsing to use periods for decimal points instead of commas.
    /// Fix for https://github.com/StateSmith/StateSmith/issues/159
    /// </summary>
    private static void AppUseDecimalPeriod()
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    }

    private void ReadDiagramRenderConfigs()
    {
        // TODO add a note about when this is undone. Presumably the transformer pass in Run does not have this set?
        ((StandardSmTransformer)transformer).onlyPreDiagramSettings = true;

        try
        {
            // Note that this may throw if the diagram is invalid.
            var preDiagramSettingsReader = preDiagramSettingsReaderProvider();
            if (preDiagramSettingsReader != null)
            {
                SetupAndFindStateMachine(inputSmBuilder, context.runnerSettings);
                preDiagramSettingsReader.Process();
            }
        }
        catch (Exception e)
        {
            OutputExceptionDetail(e);
            throw;
        }
    }

    /// <summary>
    /// Prints exception details and optionally dumps them to a file, then outputs a failure message.
    /// </summary>
    private void OutputExceptionDetail(Exception e)
    {
        var optionalErrorDetailFilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), context.runnerSettings.DiagramPath + ".err.txt");

        exceptionPrinter.PrintException(e, dumpDetailsFilePath: optionalErrorDetailFilePath);

        consolePrinter.WriteErrorLine($"Related error info/debug settings: 'dumpErrorsToFile', 'propagateExceptions'. See https://github.com/StateSmith/StateSmith/blob/main/docs/settings.md .");

        this.consolePrinter.OutputStageMessage("Finished with failure.");

        // Give stdout a chance to print the exception before exception is thrown.
        // We sometimes would see our printed detail message info cut off by dotnet exception handling.
        // See https://github.com/StateSmith/StateSmith/issues/375
        System.Threading.Thread.Sleep(100);
    }

    /// <summary>
    /// Finalizes settings.
    /// exists just for testing. can be removed in the future.
    /// </summary>
    internal void PrepareBeforeRun()
    {
        AppUseDecimalPeriod();
        this.context.callerFilePath.ThrowIfNull();
        ResolveFilePaths(context.runnerSettings, context.callerFilePath);
        outputInfo.outputDirectory = context.runnerSettings.outputDirectory.ThrowIfNull();
    }

}
