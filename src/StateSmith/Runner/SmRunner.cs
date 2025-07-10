#nullable enable

using Microsoft.Extensions.DependencyInjection;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using System;
using StateSmith.SmGraph;
using StateSmith.Output.UserConfig.AutoVars;
using StateSmith.Output.Sim;

namespace StateSmith.Runner;

/// <summary>
/// Builds a single state machine and runs code generation.
/// </summary>
/// TODO remove SmRunnerInternal by using injection in static constructor
public class SmRunner : SmRunner.IExperimentalAccess
{
    public static void ResolveFilePaths(RunnerSettings settings, string? callingFilePath)
    {
        var relativeDirectory = System.IO.Path.GetDirectoryName(callingFilePath).ThrowIfNull();
        var tmp = settings.DiagramPath;
        settings.DiagramPath = StateSmith.Common.PathUtils.EnsurePathAbsolute(settings.DiagramPath, relativeDirectory);

        settings.outputDirectory ??= System.IO.Path.GetDirectoryName(settings.DiagramPath).ThrowIfNull();
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
        var resultPath = StateSmith.Common.PathUtils.EnsurePathAbsolute(dirPath, relativeDirectory);
        resultPath = StateSmith.Common.PathUtils.EnsureDirEndingSeperator(resultPath);
        return resultPath;
    }
    /// <summary>
    /// Force application number parsing to use periods for decimal points instead of commas.
    /// Fix for https://github.com/StateSmith/StateSmith/issues/159
    /// </summary>
    public static void AppUseDecimalPeriod()
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    }
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


    public RunnerSettings Settings => context.runnerSettings;

    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// The context that holds the dynamic configuration (settings, renderconfig) for this run of the runner.
    /// </summary>
    public readonly RunnerContext context; // TODO private

    public readonly InputSmBuilder inputSmBuilder; // TODO private
    public readonly ExceptionPrinter exceptionPrinter; // TODO private
    public readonly IConsolePrinter consolePrinter; // TODO private
    public readonly Func<SimWebGenerator> simWebGeneratorProvider; // TODO private
    public readonly AlgoTranspilerCustomizer algoTranspilerCustomizer; // TODO private
    public readonly SmDesignDescriber smDesignDescriber; // TODO private
    public readonly OutputInfo outputInfo; // TODO private
    // private readonly FilePathPrinter filePathPrinter; 
    // private readonly ICodeGenRunner codeGenRunner; 
    

    /// <summary>
    /// Constructor. Mostly intended to be used by DI.
    /// </summary>
    /// <param name="context">This context object stores the runtime configuration for a given run </param>
    /// <param name="serviceProvider">Dependency injection service provider</param>
    [Obsolete("This constructor is meant for internal use only. Use SmRunner.Create() instead.")]
    public SmRunner(RunnerContext context, IServiceProvider serviceProvider, InputSmBuilder inputSmBuilder, ExceptionPrinter exceptionPrinter, IConsolePrinter consolePrinter,  Func<SimWebGenerator> simWebGeneratorProvider, AlgoTranspilerCustomizer algoTranspilerCustomizer, SmDesignDescriber smDesignDescriber, OutputInfo outputInfo/*, FilePathPrinter filePathPrinter, ICodeGenRunner codeGenRunner*/) // TODO filePathPrinter and codeGenRunner cause tests to fail
    {
        AppUseDecimalPeriod();

        this.context = context;
        this.serviceProvider = serviceProvider;
        this.inputSmBuilder = inputSmBuilder;
        this.exceptionPrinter = exceptionPrinter;
        this.consolePrinter = consolePrinter;
        this.simWebGeneratorProvider = simWebGeneratorProvider;
        this.algoTranspilerCustomizer = algoTranspilerCustomizer;
        this.smDesignDescriber = smDesignDescriber;
        this.outputInfo = outputInfo;
        // this.filePathPrinter = filePathPrinter;
        // this.codeGenRunner = codeGenRunner;

        ResolveFilePaths(context.runnerSettings, context.callerFilePath);
        SetupRenderConfigs();
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
    SmRunner.AppUseDecimalPeriod();
        this.serviceProvider = serviceProvider ?? RunnerServiceProviderFactory.CreateDefault();
        this.context = this.serviceProvider.GetRequiredService<RunnerContext>();
        this.context.runnerSettings = settings;
        this.context.renderConfig = renderConfig ?? new DummyIRenderConfig();
        this.context.callerFilePath = callerFilePath.ThrowIfNull();
    SmRunner.ResolveFilePaths(settings, this.context.callerFilePath);
        SetupRenderConfigs();
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
    /// Publicly exposed so that users can customize transformation behavior.
    /// Accessing this member will cause the Dependency Injection settings to be finalized.
    /// </summary>
    public SmTransformer SmTransformer => serviceProvider.GetRequiredService<SmTransformer>();

    /// <summary>
    /// Runs StateSmith.
    /// </summary>
    public void Run()
    {
    SmRunner.AppUseDecimalPeriod(); // done here as well to be cautious for the future

        PrepareBeforeRun();
        SmRunnerInternal smRunnerInternal = serviceProvider.GetRequiredService<SmRunnerInternal>();
        algoTranspilerCustomizer.Customize(context.runnerSettings.algorithmId, context.runnerSettings.transpilerId);
        smRunnerInternal.preDiagramBasedSettingsAlreadyApplied = serviceProvider.GetService<PreDiagramSettingsReader>() != null;

        if (context.runnerSettings.transpilerId == TranspilerId.NotYetSet)
            throw new ArgumentException("TranspilerId must be set before running code generation");

        smRunnerInternal.Run();

        if (smRunnerInternal.exception != null)
        {
            throw new FinishedWithFailureException();
        }
    
    }

    // ------------ private methods ----------------


    private void SetupRenderConfigs()
    {
        // RunnerContext is already initialized in the constructor
        ReadRenderConfigObjectToVars(context.renderConfigAllVars, context.renderConfig, context.runnerSettings.autoDeIndentAndTrimRenderConfigItems);

        try
        {
            serviceProvider.GetRequiredService<StandardSmTransformer>().onlyPreDiagramSettings = true;

            // Note that this may throw if the diagram is invalid.
            PreDiagramSettingsReader? preDiagramSettingsReader = serviceProvider.GetService<PreDiagramSettingsReader>();
            preDiagramSettingsReader?.Process();
        }
        catch (Exception e)
        {
            var smRunnerInternal = serviceProvider.GetRequiredService<SmRunnerInternal>(); // TODO move to a field?
            smRunnerInternal.OutputExceptionDetail(e);
            throw;
        }
    }


    internal static void ReadRenderConfigObjectToVars(RenderConfigAllVars renderConfigAllVars, IRenderConfig iRenderConfig, bool autoDeIndentAndTrimRenderConfigItems)
    {
        renderConfigAllVars.Base.SetFrom(iRenderConfig, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigC ircc)
            renderConfigAllVars.C.SetFrom(ircc, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigCpp irccpp)
            renderConfigAllVars.Cpp.SetFrom(irccpp, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigCSharp irccs)
            renderConfigAllVars.CSharp.SetFrom(irccs, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigJavaScript rcjs)
            renderConfigAllVars.JavaScript.SetFrom(rcjs, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigTypeScript ts)
            renderConfigAllVars.TypeScript.SetFrom(ts, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigJava rcj)
            renderConfigAllVars.Java.SetFrom(rcj, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigPython rcp)
            renderConfigAllVars.Python.SetFrom(rcp, autoDeIndentAndTrimRenderConfigItems);
    }

    /// <summary>
    /// Finalizes settings.
    /// exists just for testing. can be removed in the future.
    /// </summary>
    internal void PrepareBeforeRun()
    {
    this.context.callerFilePath.ThrowIfNull();
    SmRunner.ResolveFilePaths(context.runnerSettings, context.callerFilePath);
    OutputInfo outputInfo = serviceProvider.GetRequiredService<OutputInfo>();
    outputInfo.outputDirectory = context.runnerSettings.outputDirectory.ThrowIfNull();
    }

    // ----------- experimental access  -------------
    // exists just for now to help make it clear StateSmith API that is likely to change soon.

    public IExperimentalAccess GetExperimentalAccess() => this;
    RunnerSettings IExperimentalAccess.Settings => context.runnerSettings;
    InputSmBuilder IExperimentalAccess.InputSmBuilder => serviceProvider.GetRequiredService<InputSmBuilder>();

    /// <summary>
    /// The API in this experimental access may break often. It will eventually stabilize after enough use and feedback.
    /// </summary>
    public interface IExperimentalAccess
    {
        RunnerSettings Settings { get; }
        InputSmBuilder InputSmBuilder { get; }
    }
}
