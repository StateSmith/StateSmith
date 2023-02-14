using StateSmith.Output.C99BalancedCoder1;
using Microsoft.Extensions.DependencyInjection;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Common;

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
    readonly RenderConfigC renderConfigC;

    /// <summary>
    /// The path to the file that called a <see cref="SmRunner"/> constructor. Allows for convenient relative path
    /// figuring for regular C# projects and C# scripts (.csx).
    /// </summary>
    readonly string callerFilePath;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="callerFilePath">Don't provide this argument. C# will automatically populate it.</param>
    public SmRunner(RunnerSettings settings, [System.Runtime.CompilerServices.CallerFilePath] string? callerFilePath = null)
    {
        this.callerFilePath = callerFilePath.ThrowIfNull();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);

        this.settings = settings;
        renderConfigC = new RenderConfigC();
        renderConfigC.SetFromIRenderConfigC(this.settings.renderConfig, settings.autoDeIndentAndTrimRenderConfigItems);

        diServiceProvider = DiServiceProvider.CreateDefault();
        SetupDependencyInjection();
    }

    /// <summary>
    /// A convenience constructor.
    /// </summary>
    /// <param name="diagramPath">Relative to directory of script file that calls this constructor.</param>
    /// <param name="renderConfigC"></param>
    /// <param name="outputDirectory">Optional. If omitted, it will default to directory of <paramref name="diagramPath"/>. Relative to directory of script file that calls this constructor.</param>
    /// <param name="callingFilePath">Should normally be left unspecified so that C# can determine it automatically.</param>
    public SmRunner(string diagramPath, IRenderConfigC? renderConfigC = null, string? outputDirectory = null, [System.Runtime.CompilerServices.CallerFilePath] string? callingFilePath = null)
    : this(new RunnerSettings(renderConfigC ?? new DummyRenderConfigC(), diagramFile: diagramPath, outputDirectory: outputDirectory), callerFilePath: callingFilePath)
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

    private void SetupDependencyInjection()
    {
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
    }

    // exists just for testing. can be removed in the future.
    internal void PrepareBeforeRun()
    {
        diServiceProvider.BuildIfNeeded();
        SmRunnerInternal.ResolveFilePaths(settings, callerFilePath);
    }

    // ----------- experimental access  -------------
    // exists just for now to help make it clear StateSmith API that is likely to change soon.

    public IExperimentalAccess GetExperimentalAccess() => this;
    DiServiceProvider IExperimentalAccess.DiServiceProvider => diServiceProvider;
    RunnerSettings IExperimentalAccess.Settings => settings;
    InputSmBuilder IExperimentalAccess.InputSmBuilder => diServiceProvider.GetServiceOrCreateInstance();
    RenderConfigC IExperimentalAccess.RenderConfigC => renderConfigC;

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

        RenderConfigC RenderConfigC { get; }
    }
}


