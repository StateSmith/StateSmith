#nullable enable

using Microsoft.Extensions.DependencyInjection;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using StateSmith.SmGraph;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;

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

    private readonly IRenderConfig iRenderConfig;
    private readonly bool enablePreDiagramBasedSettings;

    /// <summary>
    /// The path to the file that called a <see cref="SmRunner"/> constructor. Allows for convenient relative path
    /// figuring for regular C# projects and C# scripts (.csx).
    /// </summary>
    readonly string callerFilePath;

    private ExceptionDispatchInfo? preDiagramBasedSettingsException = null;

    /// <summary>
    /// Constructor. Will attempt to read settings from the diagram file.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="renderConfig"></param>
    /// <param name="callerFilePath">Don't provide this argument. C# will automatically populate it.</param>
    /// <param name="enablePDBS">User code should leave unspecified for now.</param>
    public SmRunner(RunnerSettings settings, IRenderConfig? renderConfig, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null, bool enablePDBS = true)
    {
        SmRunnerInternal.AppUseDecimalPeriod();

        this.settings = settings;
        this.iRenderConfig = renderConfig ?? new DummyIRenderConfig();
        this.enablePreDiagramBasedSettings = enablePDBS;
        this.callerFilePath = callerFilePath.ThrowIfNull();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);

        diServiceProvider = DiServiceProvider.CreateDefault();
        SetupDependencyInjectionAndRenderConfigs();
    }

    /// <summary>
    /// A convenience constructor. Will attempt to read settings from the diagram file.
    /// </summary>
    /// <param name="diagramPath">Relative to directory of script file that calls this constructor.</param>
    /// <param name="renderConfig"></param>
    /// <param name="outputDirectory">Optional. If omitted, it will default to directory of <paramref name="diagramPath"/>. Relative to directory of script file that calls this constructor.</param>
    /// <param name="algorithmId">Optional. Will allow you to choose which algorithm to use when multiple are supported. Ignored if custom code generator used.</param>
    /// <param name="transpilerId">Optional. Defaults to C99. Allows you to specify which programming language to generate for. Ignored if custom code generator used.</param>
    /// <param name="callingFilePath">Should normally be left unspecified so that C# can determine it automatically.</param>
    /// <param name="enablePDBS">User could should leave unspecified for now.</param>
    public SmRunner(string diagramPath,
        IRenderConfig? renderConfig = null,
        string? outputDirectory = null,
        AlgorithmId algorithmId = AlgorithmId.Default,
        TranspilerId transpilerId = TranspilerId.Default,
        [System.Runtime.CompilerServices.CallerFilePath] string? callingFilePath = null, bool enablePDBS = true)
    : this(new RunnerSettings(diagramFile: diagramPath, outputDirectory: outputDirectory, algorithmId: algorithmId, transpilerId: transpilerId), renderConfig, callerFilePath: callingFilePath, enablePDBS: enablePDBS)
    {
    }

    /// <summary>
    /// Publicly exposed so that users can customize transformation behavior.
    /// Accessing this member will cause the Dependency Injection settings to be finalized.
    /// </summary>
    public SmTransformer SmTransformer => diServiceProvider.GetServiceOrCreateInstance();

    /// <summary>
    /// This API is experimental and may change in the future.
    /// </summary>
    public ExceptionDispatchInfo? PreDiagramBasedSettingsException => preDiagramBasedSettingsException;

    /// <summary>
    /// Runs StateSmith. Will cause the Dependency Injection settings to be finalized.
    /// </summary>
    public void Run()
    {
        SmRunnerInternal.AppUseDecimalPeriod(); // done here as well to be cautious for the future

        PrepareBeforeRun(); // finalizes dependency injection
        SmRunnerInternal smRunnerInternal = diServiceProvider.GetServiceOrCreateInstance();
        smRunnerInternal.preDiagramBasedSettingsAlreadyApplied = enablePreDiagramBasedSettings;

        // Wrap in try finally so that we can ensure that the service provider is disposed which will
        // dispose of objects that it created.
        try
        {
            PrintAndThrowIfPreDiagramSettingsException();

            if (settings.transpilerId == TranspilerId.NotYetSet)
                throw new ArgumentException("TranspilerId must be set before running code generation");

            smRunnerInternal.Run();
        }
        finally
        {
            diServiceProvider.Dispose();
        }

        if (smRunnerInternal.exception != null)
        {
            throw new FinishedWithFailureException();
        }
    }

    /// <summary>
    /// Experimental API. May change in the future.
    /// </summary>
    public void PrintAndThrowIfPreDiagramSettingsException()
    {
        if (PreDiagramBasedSettingsException != null)
        {
            // We use SmRunnerInternal to print the exception so that it is consistent with the rest of the code.
            SmRunnerInternal smRunnerInternal = diServiceProvider.GetServiceOrCreateInstance();
            smRunnerInternal.OutputExceptionDetail(PreDiagramBasedSettingsException.SourceException);
            if (settings.propagateExceptions)
            {
                PreDiagramBasedSettingsException.Throw();
            }

            throw new FinishedWithFailureException();
        }
    }

    // ------------ private methods ----------------

    private void SetupDependencyInjectionAndRenderConfigs()
    {
        var renderConfigAllVars = new RenderConfigAllVars();

        ReadRenderConfigObjectToVars(renderConfigAllVars, iRenderConfig, settings.autoDeIndentAndTrimRenderConfigItems);

        SetupDiProvider(diServiceProvider, renderConfigAllVars, settings, iRenderConfig);

        // we disable early diagram settings reading for the simulator and some tests
        if (enablePreDiagramBasedSettings)
        {
            // Why do we do this before DiServiceProvider is set up? It is a pain to not have DI set up.
            // A number of reasons. We need to read the settings before we can set up the DI provider.
            // Also (less importantly), we want to read settings from the diagram so that the user can
            // override them in a .csx file (if they choose) before running the code generator.
            // https://github.com/StateSmith/StateSmith/issues/349
            try
            {
                // Note that this may throw if the diagram is invalid.
                PreDiagramSettingsReader preDiagramSettingsReader = new(renderConfigAllVars, settings, iRenderConfig);
                preDiagramSettingsReader.Process();
            }
            catch (Exception e)
            {
                // NOTE! we can't print or log this exception here because dependency injection is not yet set up
                preDiagramBasedSettingsException = ExceptionDispatchInfo.Capture(e);
            }
        }

        AlgoOrTranspilerUpdated();
    }

    internal static void SetupDiProvider(DiServiceProvider di, RenderConfigAllVars renderConfigAllVars, RunnerSettings settings, IRenderConfig iRenderConfig)
    {
        di.AddConfiguration((services) =>
        {
            services.AddSingleton(settings.drawIoSettings);
            services.AddSingleton(settings.smDesignDescriber);
            services.AddSingleton(settings.style);
            services.AddSingleton<OutputInfo>();
            services.AddSingleton<IOutputInfo>((s) => s.GetService<OutputInfo>().ThrowIfNull());
            services.AddSingleton(renderConfigAllVars);
            services.AddSingleton(renderConfigAllVars.Base);
            services.AddSingleton(renderConfigAllVars.C);
            services.AddSingleton(renderConfigAllVars.CSharp);
            services.AddSingleton(renderConfigAllVars.JavaScript);
            services.AddSingleton(renderConfigAllVars.Java);
            services.AddSingleton(new ExpansionConfigReaderObjectProvider(iRenderConfig));
            services.AddSingleton(settings); // todo_low - split settings up more
            services.AddSingleton<ExpansionsPrep>();
            services.AddSingleton<FilePathPrinter>(new FilePathPrinter(settings.filePathPrintBase.ThrowIfNull()));
            services.AddSingleton(settings.algoBalanced1);
        });
    }

    internal static void ReadRenderConfigObjectToVars(RenderConfigAllVars renderConfigAllVars, IRenderConfig iRenderConfig, bool autoDeIndentAndTrimRenderConfigItems)
    {
        renderConfigAllVars.Base.SetFrom(iRenderConfig, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigC ircc)
            renderConfigAllVars.C.SetFrom(ircc, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigCSharp irccs)
            renderConfigAllVars.CSharp.SetFrom(irccs, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigJavaScript rcjs)
            renderConfigAllVars.JavaScript.SetFrom(rcjs, autoDeIndentAndTrimRenderConfigItems);

        if (iRenderConfig is IRenderConfigJava rcj)
            renderConfigAllVars.Java.SetFrom(rcj, autoDeIndentAndTrimRenderConfigItems);
    }

    /// <summary>
    /// You only need to call this if you adjust the algorithm or transpiler id after constructing a <see cref="SmRunner"/>.
    /// Will put in some defaults appropriate for algorithm and transpiler.
    /// </summary>
    public void AlgoOrTranspilerUpdated()
    {
        new AlgoTranspilerCustomizer().Customize(diServiceProvider, settings.algorithmId, settings.transpilerId, settings.algoBalanced1);
    }

    /// <summary>
    /// Finalizes dependency injection settings.
    /// exists just for testing. can be removed in the future.
    /// </summary>
    internal void PrepareBeforeRun()
    {
        diServiceProvider.BuildIfNeeded();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);
        OutputInfo outputInfo = diServiceProvider.GetInstanceOf<OutputInfo>();
        outputInfo.outputDirectory = settings.outputDirectory.ThrowIfNull();
    }

    // ----------- experimental access  -------------
    // exists just for now to help make it clear StateSmith API that is likely to change soon.

    public IExperimentalAccess GetExperimentalAccess() => this;
    DiServiceProvider IExperimentalAccess.DiServiceProvider => diServiceProvider;
    RunnerSettings IExperimentalAccess.Settings => settings;
    InputSmBuilder IExperimentalAccess.InputSmBuilder => diServiceProvider.GetServiceOrCreateInstance();

    /// <summary>
    /// The API in this experimental access may break often. It will eventually stabilize after enough use and feedback.
    /// </summary>
    public interface IExperimentalAccess
    {
        /// <summary>
        /// Dependency Injection Service Provider
        /// </summary>
        DiServiceProvider DiServiceProvider { get; }

        RunnerSettings Settings { get; }
        InputSmBuilder InputSmBuilder { get; }
    }
}


