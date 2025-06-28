#nullable enable

using Microsoft.Extensions.DependencyInjection;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using StateSmith.SmGraph;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using StateSmith.Output.UserConfig.AutoVars;
using System.Collections.Generic;

namespace StateSmith.Runner;

/// <summary>
/// Builds a single state machine and runs code generation.
/// </summary>
/// TODO add a static constructor
/// TODO remove SmRunnerInternal by using injection in static constructor
/// TODO use scopes to remove two stages for dependency injection
/// TODO add test case for two smrunners at same time (to make sure singletons are not shared between them)
public class SmRunner : SmRunner.IExperimentalAccess
{
    public RunnerSettings Settings => settings;

    /// <summary>
    /// Dependency Injection ServiceProvider Builder.
    /// We can't use the IServiceProvider directly because we need to add the 
    /// dependencies that are based on RenderConfig and RunnerSettings.
    /// </summary>
    readonly IConfigServiceProviderBuilder serviceProviderBuilder;

    // TODO remove ? once it's guaranteed to be non-null
    private IServiceProvider? serviceProvider;

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
    /// <param name="serviceOverrides">Optional dependency injection overrides</param>
    /// <param name="callerFilePath">Don't provide this argument. C# will automatically populate it.</param>
    /// <param name="enablePDBS">User code should leave unspecified for now.</param>
    public SmRunner(RunnerSettings settings, IRenderConfig? renderConfig, IConfigServiceProviderBuilder? serviceProviderBuilder = null, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null, bool enablePDBS = true)
    {
        SmRunnerInternal.AppUseDecimalPeriod();

        this.settings = settings;
        this.iRenderConfig = renderConfig ?? new DummyIRenderConfig();
        this.enablePreDiagramBasedSettings = enablePDBS;
        this.callerFilePath = callerFilePath.ThrowIfNull();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);

        this.serviceProviderBuilder = serviceProviderBuilder ?? IConfigServiceProviderBuilder.CreateDefault();

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
    /// <param name="serviceProviderBuilder">Optional builder for the IServiceProvider</param>
    public SmRunner(string diagramPath,
        IRenderConfig? renderConfig = null,
        string? outputDirectory = null,
        AlgorithmId algorithmId = AlgorithmId.Default,
        TranspilerId transpilerId = TranspilerId.Default,
        IConfigServiceProviderBuilder? serviceProviderBuilder = null,
        [System.Runtime.CompilerServices.CallerFilePath] string? callingFilePath = null, bool enablePDBS = true)
    : this(new RunnerSettings(diagramFile: diagramPath, outputDirectory: outputDirectory, algorithmId: algorithmId, transpilerId: transpilerId), renderConfig, serviceProviderBuilder, callerFilePath: callingFilePath, enablePDBS: enablePDBS)
    {
    }

    /// <summary>
    /// Publicly exposed so that users can customize transformation behavior.
    /// Accessing this member will cause the Dependency Injection settings to be finalized.
    /// </summary>
    public SmTransformer SmTransformer => ActivatorUtilities.GetServiceOrCreateInstance<SmTransformer>(serviceProvider);

    /// <summary>
    /// This API is experimental and may change in the future.
    /// </summary>
    public ExceptionDispatchInfo? PreDiagramBasedSettingsException => preDiagramBasedSettingsException;

    /// <summary>
    /// Runs StateSmith.
    /// </summary>
    public void Run()
    {
        // TODO remove SmRunner direct use of DiServiceProvider, use IServiceProvider instead
        // TODO move DI finalization out of SmRunner

        SmRunnerInternal.AppUseDecimalPeriod(); // done here as well to be cautious for the future

        PrepareBeforeRun();
        SmRunnerInternal smRunnerInternal = serviceProvider!.GetRequiredService<SmRunnerInternal>();
        smRunnerInternal.preDiagramBasedSettingsAlreadyApplied = enablePreDiagramBasedSettings;

        // Wrap in try finally so that we can ensure that the service provider is disposed which will
        // dispose of objects that it created.
        // TODO remove
        try
        {
            PrintAndThrowIfPreDiagramSettingsException();

            if (settings.transpilerId == TranspilerId.NotYetSet)
                throw new ArgumentException("TranspilerId must be set before running code generation");

            smRunnerInternal.Run();
        }
        finally
        {
            // TODO remove this once IHost lifecycle is managed outside of SmRunner.
            serviceProviderBuilder.Dispose();
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
            SmRunnerInternal smRunnerInternal = ActivatorUtilities.GetServiceOrCreateInstance<SmRunnerInternal>(serviceProvider);
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

        // TODO move this higher so DI is available during the prediagram settings reading
        serviceProvider = serviceProviderBuilder
            // TODO remove renderConfigAllVars from here, it is always(?) initialized using iRenderConfig
            .WithRenderConfig(renderConfigAllVars, iRenderConfig)
            .WithRunnerSettings(settings)
            .Build();

        ReadRenderConfigObjectToVars(renderConfigAllVars, iRenderConfig, settings.autoDeIndentAndTrimRenderConfigItems);

        // we disable early diagram settings reading for the simulator and some tests
        if (enablePreDiagramBasedSettings)
        {
            // Why do we do this before DiServiceProvider is set up? It is a pain to not have DI set up.
            // A number of reasons. We need to read the settings before we can set up the DI provider.
            // Also (less importantly), we want to read settings from the diagram so that the user can
            // override them in a .csx file (if they choose) before running the code generator.
            // https://github.com/StateSmith/StateSmith/issues/349
            // TODO update comment
            try
            {
                // Note that this may throw if the diagram is invalid.
                // TODO inject PreDiagramSettingsReader
                PreDiagramSettingsReader preDiagramSettingsReader = new(renderConfigAllVars, settings, iRenderConfig);
                preDiagramSettingsReader.Process();
            }
            catch (Exception e)
            {
                // NOTE! we can't print or log this exception here because dependency injection is not yet set up
                // TODO remove this try catch and preDiagramBasedsettingsException once we have DI set up
                preDiagramBasedSettingsException = ExceptionDispatchInfo.Capture(e);
            }
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
    /// Finalizes dependency injection settings.
    /// TODO fix summary
    /// exists just for testing. can be removed in the future.
    /// </summary>
    internal void PrepareBeforeRun()
    {
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);
        OutputInfo outputInfo = serviceProvider!.GetRequiredService<OutputInfo>();
        outputInfo.outputDirectory = settings.outputDirectory.ThrowIfNull();
    }

    // ----------- experimental access  -------------
    // exists just for now to help make it clear StateSmith API that is likely to change soon.

    public IExperimentalAccess GetExperimentalAccess() => this;
    IServiceProvider IExperimentalAccess.IServiceProvider => serviceProvider;
    RunnerSettings IExperimentalAccess.Settings => settings;
    InputSmBuilder IExperimentalAccess.InputSmBuilder => ActivatorUtilities.GetServiceOrCreateInstance<InputSmBuilder>(serviceProvider);

    /// <summary>
    /// The API in this experimental access may break often. It will eventually stabilize after enough use and feedback.
    /// </summary>
    public interface IExperimentalAccess
    {

        IServiceProvider IServiceProvider { get; }

        RunnerSettings Settings { get; }
        InputSmBuilder InputSmBuilder { get; }
    }
}


