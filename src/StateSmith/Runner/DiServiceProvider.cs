#nullable enable

using StateSmith.Input.DrawIo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using StateSmith.Input.Expansions;
using StateSmith.SmGraph;
using StateSmith.Output.UserConfig;
using StateSmith.Output;
using StateSmith.Common;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.SmGraph.TriggerMap;
using StateSmith.Output.Sim;
using StateSmith.Output.Gil;
using StateSmith.Output.Algos.Balanced2;
using StateSmith.Output.UserConfig.AutoVars;
using StateSmith.Output.Gil.Cpp;

namespace StateSmith.Runner;

/// <summary>
/// Dependency Injection Service Provider
/// </summary>
public class DiServiceProvider : IDisposable
{
    // TODO temporarily made public
    public IHost? host;
    private readonly IHostBuilder hostBuilder;

    public DiServiceProvider()
    {
        hostBuilder = Host.CreateDefaultBuilder();
    }

    public static DiServiceProvider CreateDefault()
    {
        DiServiceProvider sp = new();
        sp.SetupAsDefault();
        return sp;
    }

    public void SetupAsDefault()
    {
        hostBuilder.ConfigureServices((services) =>
        {
            AddDefaults(services);

            services.AddSingleton<DiServiceProvider>(this); // todo_low remove. See https://github.com/StateSmith/StateSmith/issues/97
            services.AddSingleton<SmRunnerInternal>();
            services.AddSingleton<SmTransformer, StandardSmTransformer>();
            services.AddSingleton<IExpander, Expander>();
            services.AddSingleton<InputSmBuilder>();
            services.AddSingleton<IConsolePrinter, ConsolePrinter>();
            services.AddSingleton<ExceptionPrinter>();
            services.AddSingleton<ICodeFileWriter, CodeFileWriter>();

            services.AddSingleton<StateMachineProvider>();
            services.AddSingleton<IStateMachineProvider>((s) => s.GetService<StateMachineProvider>()!); // need to use lambda or else another object will be created
            services.AddSingleton(new DiagramFilePathProvider());
            services.AddSingleton<SmFileNameProcessor>();

            services.AddSingleton<DiagramToSmConverter>();
            services.AddSingleton<IDiagramVerticesProvider>((s) => s.GetService<DiagramToSmConverter>()!); // need to use lambda or else another `DiagramToSmConverter` is created.
            services.AddSingleton<AlgoBalanced1Settings>();
            services.AddSingleton<IAlgoStateIdToString, AlgoStateIdToString>();
            services.AddSingleton<IAlgoEventIdToString, AlgoEventIdToString>();
            services.AddSingleton<GilToC99Customizer>();
            services.AddSingleton<IGilToC99Customizer>((s) => s.GetService<GilToC99Customizer>()!); // need to use lambda or else another `DiagramToSmConverter` is created.
            services.AddSingleton<CppGilHelpers>();

            services.AddTransient<AutoExpandedVarsProcessor>();
            services.AddTransient<DefaultExpansionsProcessor>();
            services.AddTransient<TomlConfigVerticesProcessor>();
            services.AddTransient<RenderConfigVerticesProcessor>();
            services.AddTransient<MxCellsToSmDiagramConverter>();
            services.AddTransient<DrawIoToSmDiagramConverter>();
            services.AddTransient<VisualGroupingValidator>();
            services.AddTransient<DynamicVarsResolver>();
            services.AddTransient<ExpansionConfigReader>();

            services.AddTransient<HistoryProcessor>();

            services.AddSingleton<ICodeGenRunner, GilAlgoCodeGen>();
            services.AddSingleton<IGilAlgo, AlgoBalanced1>();
            services.AddSingleton<IGilTranspiler, GilToC99>();
#if SS_SINGLE_FILE_APPLICATION
            services.AddSingleton<IRoslynMetadataProvider, InMemoryMetaDataProvider>();
#else
            services.AddSingleton<IRoslynMetadataProvider, FileMetadataProvider>();
#endif
            services.AddSingleton<RoslynCompiler>();
            services.AddSingleton<NameMangler>();
            services.AddSingleton<PseudoStateHandlerBuilder>();
            services.AddSingleton<EnumBuilder>();
            services.AddSingleton<EventHandlerBuilder>();
            services.AddSingleton<EventHandlerBuilder2>();

            services.AddSingleton<StateNameConflictResolver>();
            services.AddSingleton<StandardFileHeaderPrinter>();

            services.AddSingleton<IAutoVarsParser, CLikeAutoVarsParser>();
            services.AddSingleton<TriggerMapProcessor>();

            services.AddSingleton<UserExpansionScriptBases>();
            services.AddSingleton<SmDesignDescriber>();
            services.AddSingleton<SimWebGenerator>();
        });
    }

    public void AddConfiguration(Action<IServiceCollection> services)
    {
        ThrowIfAlreadyBuilt();
        hostBuilder.ConfigureServices(services);
    }

    // only for test code
    internal void AddSingleton(InputSmBuilder obj)
    {
        ThrowIfAlreadyBuilt();
        hostBuilder.ConfigureServices(services => { services.AddSingleton(obj); });
    }

    public void AddSingletonT<TService>(TService implementationObj) where TService : class
    {
        ThrowIfAlreadyBuilt();
        hostBuilder.ConfigureServices(services => { services.AddSingleton(implementationObj); });
    }

    public void AddSingletonT<TService, TImplementation>()
    where TService : class
    where TImplementation : class, TService
    {
        ThrowIfAlreadyBuilt();
        hostBuilder.ConfigureServices(services => { services.AddSingleton<TService, TImplementation>(); });
    }

    /// <summary>
    /// Can only be done once. Limitation of lib.
    /// </summary>
    public void Build()
    {
        host = hostBuilder.Build(); // this will throw an exception if already built
    }

    public void BuildIfNeeded()
    {
        if (!IsAlreadyBuilt())
            Build();
    }

    private void ThrowIfAlreadyBuilt()
    {
        if (IsAlreadyBuilt())
        {
            throw new InvalidOperationException("Can't add after built");
        }
    }

    private bool IsAlreadyBuilt()
    {
        return host != null;
    }

    private static void AddDefaults(IServiceCollection services)
    {
        // TODO move scoped declarations elsewhere?
        // RenderConfigAllVars is essentially a singleton in the scope.
        // RenderConfigAllVars.Base, .C, .Cpp, etc. are all obtained from that singleton
        services.AddScoped<RenderConfigAllVars,RenderConfigAllVars>();
        services.AddScoped(sp => sp.GetService<RenderConfigAllVars>().Base);
        services.AddScoped(sp => sp.GetService<RenderConfigAllVars>().C);
        services.AddScoped(sp => sp.GetService<RenderConfigAllVars>().Cpp);
        services.AddScoped(sp => sp.GetService<RenderConfigAllVars>().CSharp);
        services.AddScoped(sp => sp.GetService<RenderConfigAllVars>().JavaScript);
        services.AddScoped(sp => sp.GetService<RenderConfigAllVars>().Java);
        services.AddScoped(sp => sp.GetService<RenderConfigAllVars>().Python);
        services.AddScoped(sp => sp.GetService<RenderConfigAllVars>().TypeScript);


        services.AddSingleton(new DrawIoSettings());
        services.AddSingleton(new CodeStyleSettings());
        services.AddSingleton<IExpansionVarsPathProvider, CSharpExpansionVarsPathProvider>();
        services.AddSingleton<RunnerSettings>(new RunnerSettings(""));
        services.AddSingleton<FilePathPrinter>(new FilePathPrinter(""));
        services.AddSingleton(new SmDesignDescriberSettings());
    }

    /// <summary>
    /// This class has implicit conversions that give some compile time type safety to <see cref="DiServiceProvider.GetServiceOrCreateInstance"/>.
    /// Might remove this class.
    /// </summary>
    public class ConvertableType
    {
        public IHost host;

        public ConvertableType(IHost host)
        {
            this.host = host;
        }
        
        public static implicit operator StateMachineProvider(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<StateMachineProvider>(me.host.Services);
        public static implicit operator SmRunnerInternal(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<SmRunnerInternal>(me.host.Services);
        public static implicit operator DrawIoToSmDiagramConverter(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<DrawIoToSmDiagramConverter>(me.host.Services);
        public static implicit operator DiagramToSmConverter(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<DiagramToSmConverter>(me.host.Services);
        public static implicit operator DrawIoSettings(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<DrawIoSettings>(me.host.Services);
        public static implicit operator SmTransformer(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<SmTransformer>(me.host.Services);
        public static implicit operator RenderConfigBaseVars(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<RenderConfigBaseVars>(me.host.Services);
        public static implicit operator RenderConfigCVars(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<RenderConfigCVars>(me.host.Services);
        public static implicit operator RenderConfigCSharpVars(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<RenderConfigCSharpVars>(me.host.Services);
        public static implicit operator InputSmBuilder(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<InputSmBuilder>(me.host.Services);
    }

    /// <summary>
    /// Should ideally only be used by code that sets up Service Provider and can't use dependency injection.
    /// Otherwise, it can hide dependencies. See https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/ .
    /// </summary>
    /// <returns></returns>
    public T GetInstanceOf<T>()
    {
        BuildIfNeeded();
        return ActivatorUtilities.GetServiceOrCreateInstance<T>(host.ThrowIfNull().Services);
    }

    /// <summary>
    /// Should ideally only be used by code that sets up Service Provider and can't use dependency injection.
    /// Otherwise, it can hide dependencies. See https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/ .
    /// </summary>
    /// <returns></returns>
    internal ConvertableType GetServiceOrCreateInstance()
    {
        BuildIfNeeded();
        return new ConvertableType(host.ThrowIfNull());
    }

    public void Dispose()
    {
        host?.Dispose();
    }
}

