using Microsoft.Extensions.DependencyInjection;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Common;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.Algos.Balanced1;

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

    private readonly IRenderConfig iRenderConfig;

    /// <summary>
    /// The path to the file that called a <see cref="SmRunner"/> constructor. Allows for convenient relative path
    /// figuring for regular C# projects and C# scripts (.csx).
    /// </summary>
    readonly string callerFilePath;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="renderConfig"></param>
    /// <param name="callerFilePath">Don't provide this argument. C# will automatically populate it.</param>
    public SmRunner(RunnerSettings settings, IRenderConfig? renderConfig, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        this.settings = settings;
        this.iRenderConfig = renderConfig ?? new DummyIRenderConfig();
        this.callerFilePath = callerFilePath.ThrowIfNull();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);

        diServiceProvider = DiServiceProvider.CreateDefault();
        SetupDependencyInjectionAndRenderConfigs();
    }

    /// <summary>
    /// A convenience constructor.
    /// </summary>
    /// <param name="diagramPath">Relative to directory of script file that calls this constructor.</param>
    /// <param name="renderConfig"></param>
    /// <param name="outputDirectory">Optional. If omitted, it will default to directory of <paramref name="diagramPath"/>. Relative to directory of script file that calls this constructor.</param>
    /// <param name="callingFilePath">Should normally be left unspecified so that C# can determine it automatically.</param>
    public SmRunner(string diagramPath, IRenderConfig? renderConfig = null, string? outputDirectory = null, [System.Runtime.CompilerServices.CallerFilePath] string? callingFilePath = null)
    : this(new RunnerSettings(diagramFile: diagramPath, outputDirectory: outputDirectory), renderConfig, callerFilePath: callingFilePath)
    {
    }

    /// <summary>
    /// Publicly exposed so that users can customize transformation behavior.
    /// Accessing this member will cause the Dependency Injection settings to be finalized.
    /// </summary>
    public SmTransformer SmTransformer => diServiceProvider.GetServiceOrCreateInstance();


    /// <summary>
    /// Runs StateSmith. Will cause the Dependency Injection settings to be finalized.
    /// </summary>
    public void Run()
    {
        PrepareBeforeRun();
        SmRunnerInternal smRunnerInternal = diServiceProvider.GetServiceOrCreateInstance();
        smRunnerInternal.Run();

        if (smRunnerInternal.exception != null)
            throw new System.Exception("Finished with failure");
    }

    // ------------ private methods ----------------

    private void SetupDependencyInjectionAndRenderConfigs()
    {
        var renderConfigVars = new RenderConfigVars();
        renderConfigVars.SetFrom(iRenderConfig, settings.autoDeIndentAndTrimRenderConfigItems);

        var renderConfigCVars = new RenderConfigCVars();
        if (iRenderConfig is IRenderConfigC ircc)
        {
            renderConfigCVars.SetFrom(ircc, settings.autoDeIndentAndTrimRenderConfigItems);
        }

        var renderConfigCSharpVars = new RenderConfigCSharpVars();
        if (iRenderConfig is IRenderConfigCSharp irccs)
        {
            renderConfigCSharpVars.SetFrom(irccs, settings.autoDeIndentAndTrimRenderConfigItems);
        }

        diServiceProvider.AddConfiguration((services) =>
        {
            services.AddSingleton(settings.drawIoSettings);
            services.AddSingleton(settings.style);
            services.AddSingleton<OutputInfo>();
            services.AddSingleton<RenderConfigVars>(renderConfigVars);
            services.AddSingleton<RenderConfigCVars>(renderConfigCVars);
            services.AddSingleton<RenderConfigCSharpVars>(renderConfigCSharpVars);
            services.AddSingleton(new ExpansionConfigReaderObjectProvider(iRenderConfig));
            services.AddSingleton(settings); // todo_low - split settings up more
            services.AddSingleton<ExpansionsPrep>();
            services.AddSingleton<IExpansionVarsPathProvider, CExpansionVarsPathProvider>(); // FIXME - set based on target language
        });
    }

    // exists just for testing. can be removed in the future.
    internal void PrepareBeforeRun()
    {
        diServiceProvider.BuildIfNeeded();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);
        diServiceProvider.GetInstanceOf<OutputInfo>().outputDirectory = settings.outputDirectory;
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


