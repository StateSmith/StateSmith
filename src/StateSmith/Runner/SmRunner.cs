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
/// TODO add a static constructor
/// TODO remove SmRunnerInternal by using injection in static constructor
/// TODO add test case for two smrunners at same time (to make sure singletons are not shared between them)
public class SmRunner : SmRunner.IExperimentalAccess
{
    public RunnerSettings Settings => settings;

    private IServiceProvider serviceProvider;

    readonly RunnerSettings settings;

    private readonly IRenderConfig iRenderConfig;
    
    private readonly bool enablePreDiagramBasedSettings;

    /// <summary>
    /// The path to the file that called a <see cref="SmRunner"/> constructor. Allows for convenient relative path
    /// figuring for regular C# projects and C# scripts (.csx).
    /// </summary>
    readonly string callerFilePath;

    /// <summary>
    /// Constructor. Will attempt to read settings from the diagram file.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="renderConfig"></param>
    /// <param name="serviceProvider">Optional dependency injection overrides</param>
    /// <param name="callerFilePath">Don't provide this argument. C# will automatically populate it.</param>
    /// <param name="enablePDBS">User code should leave unspecified for now.</param>
    public SmRunner(RunnerSettings settings, IRenderConfig? renderConfig, IServiceProvider? serviceProvider = null, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null, bool enablePDBS = true)
    {
        SmRunnerInternal.AppUseDecimalPeriod();

        this.settings = settings;
        this.iRenderConfig = renderConfig ?? new DummyIRenderConfig();
        this.enablePreDiagramBasedSettings = enablePDBS;
        this.callerFilePath = callerFilePath.ThrowIfNull();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);

        this.serviceProvider = serviceProvider ?? RunnerServiceProviderFactory.CreateDefault();

        SetupRenderConfigs();
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
    /// <param name="serviceProvider">Optional IServiceProvider to override bindings</param>
    public SmRunner(string diagramPath,
        IRenderConfig? renderConfig = null,
        string? outputDirectory = null,
        AlgorithmId algorithmId = AlgorithmId.Default,
        TranspilerId transpilerId = TranspilerId.Default,
        IServiceProvider? serviceProvider = null,
        [System.Runtime.CompilerServices.CallerFilePath] string? callingFilePath = null, bool enablePDBS = true)
    : this(new RunnerSettings(diagramFile: diagramPath, outputDirectory: outputDirectory, algorithmId: algorithmId, transpilerId: transpilerId), renderConfig, serviceProvider, callerFilePath: callingFilePath, enablePDBS: enablePDBS)
    {
    }

    /// <summary>
    /// Publicly exposed so that users can customize transformation behavior.
    /// Accessing this member will cause the Dependency Injection settings to be finalized.
    /// </summary>
    public SmTransformer SmTransformer => ActivatorUtilities.GetServiceOrCreateInstance<SmTransformer>(serviceProvider);

    /// <summary>
    /// Runs StateSmith.
    /// </summary>
    public void Run()
    {
        SmRunnerInternal.AppUseDecimalPeriod(); // done here as well to be cautious for the future

        PrepareBeforeRun();
        SmRunnerInternal smRunnerInternal = serviceProvider.GetRequiredService<SmRunnerInternal>();
        smRunnerInternal.preDiagramBasedSettingsAlreadyApplied = enablePreDiagramBasedSettings;

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

        // we disable early diagram settings reading for the simulator and some tests
        if (enablePreDiagramBasedSettings)
        {
            try
            {
                serviceProvider.GetRequiredService<StandardSmTransformer>().onlyPreDiagramSettings = true;

                // Note that this may throw if the diagram is invalid.
                PreDiagramSettingsReader preDiagramSettingsReader = serviceProvider.GetRequiredService<PreDiagramSettingsReader>();
                preDiagramSettingsReader.Process();
            }
            catch (Exception e)
            {
                var smRunnerInternal = serviceProvider.GetRequiredService<SmRunnerInternal>(); // TODO move to a field?
                smRunnerInternal.OutputExceptionDetail(e); // TODO why is this needed?
                throw;
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
    /// Finalizes settings.
    /// exists just for testing. can be removed in the future.
    /// // TODO remove
    /// </summary>
    internal void PrepareBeforeRun()
    {
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);
        OutputInfo outputInfo = serviceProvider.GetRequiredService<OutputInfo>();
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
