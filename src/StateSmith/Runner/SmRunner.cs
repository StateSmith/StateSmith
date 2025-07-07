#nullable enable

using Microsoft.Extensions.DependencyInjection;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using System;
using StateSmith.SmGraph;
using StateSmith.Output.UserConfig.AutoVars;

namespace StateSmith.Runner;

/// <summary>
/// Builds a single state machine and runs code generation.
/// </summary>
/// TODO remove SmRunnerInternal by using injection in static constructor
/// TODO add test case for two smrunners at same time (to make sure singletons are not shared between them)
public class SmRunner : SmRunner.IExperimentalAccess
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

        SmRunner smRunner = sp.GetRequiredService<SmRunner>();

        smRunner.callerFilePath = callerFilePath.ThrowIfNull(); // callerPath is set automatically if it's null

        SmRunnerInternal.ResolveFilePaths(context.runnerSettings, callerFilePath);
        smRunner.SetupRenderConfigs();

        return smRunner;
    }

    public RunnerSettings Settings => settings;

    private IServiceProvider serviceProvider;

    // TODO replace with context
    readonly RunnerSettings settings;

    // TODO replace with context
    private readonly IRenderConfig iRenderConfig;
    
    /// <summary>
    /// The path to the file that called a <see cref="SmRunner"/> constructor. Allows for convenient relative path
    /// figuring for regular C# projects and C# scripts (.csx).
    /// May be null during construction but is expected to be non-null at the time of Run
    /// </summary>
    private string? callerFilePath;

    /// <summary>
    /// Constructor. Will attempt to read settings from the diagram file.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="renderConfig"></param>
    /// <param name="serviceProvider">Dependency injection service provider</param>
    public SmRunner(RunnerSettings settings, IRenderConfig renderConfig, IServiceProvider serviceProvider)
    {
        SmRunnerInternal.AppUseDecimalPeriod();

        this.settings = settings;
        this.iRenderConfig = renderConfig ?? new DummyIRenderConfig();
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Legacy constructor provided for backward compatibility with older CSX scripts
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="renderConfig"></param>
    /// <param name="callerFilePath">Don't provide this argument. C# will automatically populate it.</param>
    /// <param name="enablePDBS">User code should leave unspecified for now.</param>
    /// <summary>
    [Obsolete("This constructor is obsolete. Use SmRunner.Create() instead.")]   
    public SmRunner(RunnerSettings settings, IRenderConfig? renderConfig, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        SmRunnerInternal.AppUseDecimalPeriod();

        this.settings = settings;
        this.iRenderConfig = renderConfig ?? new DummyIRenderConfig();
        this.callerFilePath = callerFilePath.ThrowIfNull();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);

        this.serviceProvider = RunnerServiceProviderFactory.CreateDefault();
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
    /// <param name="callingFilePath">Should normally be left unspecified so that C# can determine it automatically.</param>
    public SmRunner(string diagramPath,
        IRenderConfig? renderConfig = null,
        string? outputDirectory = null,
        AlgorithmId algorithmId = AlgorithmId.Default,
        TranspilerId transpilerId = TranspilerId.Default,
        string? callingFilePath = null)
#pragma warning disable CS0618 // Type or member is obsolete
    : this(new RunnerSettings(diagramFile: diagramPath, outputDirectory: outputDirectory, algorithmId: algorithmId, transpilerId: transpilerId), renderConfig, callerFilePath: callingFilePath)
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
        SmRunnerInternal.AppUseDecimalPeriod(); // done here as well to be cautious for the future

        PrepareBeforeRun();
        SmRunnerInternal smRunnerInternal = serviceProvider.GetRequiredService<SmRunnerInternal>();
        smRunnerInternal.preDiagramBasedSettingsAlreadyApplied = serviceProvider.GetService<PreDiagramSettingsReader>() != null;

        if (settings.transpilerId == TranspilerId.NotYetSet)
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
        // Initialize the RunnerContext with the settings for this run
        var context = serviceProvider.GetRequiredService<RunnerContext>();
        context.runnerSettings = settings;
        context.renderConfig = iRenderConfig;

        ReadRenderConfigObjectToVars(context.renderConfigAllVars, iRenderConfig, settings.autoDeIndentAndTrimRenderConfigItems);

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
    /// // TODO remove
    /// </summary>
    internal void PrepareBeforeRun()
    {
        this.callerFilePath.ThrowIfNull();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);
        OutputInfo outputInfo = serviceProvider.GetRequiredService<OutputInfo>();
        outputInfo.outputDirectory = settings.outputDirectory.ThrowIfNull();
    }

    // ----------- experimental access  -------------
    // exists just for now to help make it clear StateSmith API that is likely to change soon.

    public IExperimentalAccess GetExperimentalAccess() => this;
    IServiceProvider IExperimentalAccess.IServiceProvider => serviceProvider;
    RunnerSettings IExperimentalAccess.Settings => settings;
    InputSmBuilder IExperimentalAccess.InputSmBuilder => serviceProvider.GetRequiredService<InputSmBuilder>();

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
