
#nullable enable

using StateSmith.Input.DrawIo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
using StateSmith.Output.Gil.Python;
using StateSmith.Output.Gil.TypeScript;
using StateSmith.Output.Gil.JavaScript;
using StateSmith.Output.Gil.Java;
using StateSmith.Output.Gil.CSharp;

namespace StateSmith.Runner;


// TODO do I need the interfaces?
public interface IServiceProviderBuilder
{
    public abstract IServiceProviderBuilder WithServices(Action<IServiceCollection> services);
    public abstract IServiceProvider Build();
}

public interface IConfigServiceProviderBuilder : IServiceProviderBuilder
{
    public abstract IConfigServiceProviderBuilder WithRunnerSettings(RunnerSettings settings);
    public abstract IConfigServiceProviderBuilder WithRenderConfig(RenderConfigAllVars renderConfigAllVars, IRenderConfig iRenderConfig);
}

/// <summary>
/// Dependency Injection Service Provider
/// </summary>


// TODO rename ConfigServiceProviderBuilder
public class DiServiceProvider : IDisposable, IConfigServiceProviderBuilder
{
    // Helper to resolve a service by id from a type map
    private static TService ResolveServiceFromRunnerSettings<TService, TId>(IServiceProvider sp, Func<RunnerSettings, TId> idSelector, IReadOnlyDictionary<TId, Type> typeMap)
    {
        var settings = sp.GetRequiredService<RunnerSettings>();
        var id = idSelector(settings);
        Type t = typeMap[id].ThrowIfNull($"{id?.GetType()} '{id}' is not supported.");
        return (TService)ActivatorUtilities.GetServiceOrCreateInstance(sp, t);
    }

    private IHost? host;
    private readonly IHostBuilder hostBuilder;

    public DiServiceProvider()
    {
        hostBuilder = Host.CreateDefaultBuilder();
    }

    public static DiServiceProvider CreateDefault(Action<IServiceCollection>? serviceOverrides = null)
    {
        DiServiceProvider sp = new();
        sp.SetupAsDefault(serviceOverrides);
        return sp;
    }

    public IServiceProvider ServiceProvider => host.ThrowIfNull().Services;

    // TODO remove or make private
    public void SetupAsDefault(Action<IServiceCollection>? serviceOverrides = null)
    {
        WithServices((services) =>
        {
            services.AddSingleton(new DrawIoSettings());
            services.AddSingleton(new CodeStyleSettings());
            services.AddSingleton<RunnerSettings>(new RunnerSettings(""));
            services.AddSingleton<FilePathPrinter>(new FilePathPrinter(""));
            services.AddSingleton(new SmDesignDescriberSettings());

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
            services.AddSingleton<IGilAlgo>(sp =>
                ResolveServiceFromRunnerSettings<IGilAlgo, AlgorithmId>(sp, rs => rs.algorithmId, IGILALGO_TYPES)
            );

            services.AddSingleton<IGilTranspiler>(sp =>
                ResolveServiceFromRunnerSettings<IGilTranspiler, TranspilerId>(sp, rs => rs.transpilerId, IGILTRANSPILER_TYPES)
            );

            services.AddSingleton<IExpansionVarsPathProvider>(sp =>
                ResolveServiceFromRunnerSettings<IExpansionVarsPathProvider, TranspilerId>(sp, rs => rs.transpilerId, IEXPANSIONVARSPATHPROVIDER_TYPES)
            );

            services.AddSingleton<INameMangler>(sp =>
                ResolveServiceFromRunnerSettings<INameMangler, TranspilerId>(sp, rs => rs.transpilerId, INAMEMANGLER_TYPES)
            );

            services.AddSingleton<IAutoVarsParser>(sp =>
                ResolveServiceFromRunnerSettings<IAutoVarsParser, TranspilerId>(sp, rs => rs.transpilerId, IAUTOVARSPARSER_TYPES)
            );


#if SS_SINGLE_FILE_APPLICATION
            services.AddSingleton<IRoslynMetadataProvider, InMemoryMetaDataProvider>();
#else
            services.AddSingleton<IRoslynMetadataProvider, FileMetadataProvider>();
#endif
            services.AddSingleton<RoslynCompiler>();

            services.AddSingleton<PseudoStateHandlerBuilder>();
            services.AddSingleton<EnumBuilder>();
            services.AddSingleton<EventHandlerBuilder>();
            services.AddSingleton<EventHandlerBuilder2>();

            services.AddSingleton<StateNameConflictResolver>();
            services.AddSingleton<StandardFileHeaderPrinter>();

            services.AddSingleton<TriggerMapProcessor>();

            services.AddSingleton<UserExpansionScriptBases>();
            services.AddSingleton<SmDesignDescriber>();
            services.AddSingleton<SimWebGenerator>();

            // Merge the overrides into the service collection.
            serviceOverrides?.Invoke(services);
        });

        WithRenderConfig(null, null);
    }

    // TODO remove
    public IServiceProviderBuilder WithServices(Action<IServiceCollection> services)
    {
        hostBuilder.ConfigureServices(services);
        return this;
    }

    // TODO remove
    [Obsolete("This method is deprecated. Use SmRunner(serviceOverrides) instead.")]
    public void AddSingletonT<TService>(TService implementationObj) where TService : class
    {
        hostBuilder.ConfigureServices(services => { services.AddSingleton(implementationObj); });
    }


    /// <summary>
    /// Can only be done once. Limitation of lib.
    /// </summary>
    public IServiceProvider Build()
    {
        host = hostBuilder.Build(); // this will throw an exception if already built
        return host.Services;
    }

    public IConfigServiceProviderBuilder WithRunnerSettings(RunnerSettings settings)
    {
        WithServices(services =>
        {
            services.AddSingleton<RunnerSettings>(settings);
        });   
        
        return this;
    }

    public IConfigServiceProviderBuilder WithRenderConfig(RenderConfigAllVars? renderConfigAllVars = null, IRenderConfig? iRenderConfig = null)
    {
        // TODO do I need iRenderConfig?
        // TODO remove ? from iRenderConfig

        WithServices(services =>
        {
            // RenderConfigAllVars.Base, .C, .Cpp, etc. are all obtained from RenderConfigAllVars.
            if (renderConfigAllVars != null)
            {
                services.AddSingleton<RenderConfigAllVars>(renderConfigAllVars);
            }
            else
            {
                services.AddSingleton<RenderConfigAllVars, RenderConfigAllVars>();
            }

            services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().Base);
            services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().C);
            services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().Cpp);
            services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().CSharp);
            services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().JavaScript);
            services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().Java);
            services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().Python);
            services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().TypeScript);
        });

        return this;
    }



    public void Dispose()
    {
        host?.Dispose();
    }

    // TODO move
    Dictionary<TranspilerId, Type> IGILTRANSPILER_TYPES = new Dictionary<TranspilerId, Type> {
        { TranspilerId.Default, typeof(GilToC99)},
        { TranspilerId.Cpp, typeof(GilToCpp)},
        { TranspilerId.C99, typeof(GilToC99)},
        { TranspilerId.CSharp, typeof(GilToCSharp)},
        { TranspilerId.JavaScript, typeof(GilToJavaScript)},
        { TranspilerId.Java, typeof(GilToJava)},
        { TranspilerId.Python, typeof(GilToPython)},
        { TranspilerId.TypeScript, typeof(GilToTypeScript)}
    };

    Dictionary<TranspilerId, Type> IEXPANSIONVARSPATHPROVIDER_TYPES = new Dictionary<TranspilerId, Type>
    {
        { TranspilerId.Default, typeof(CExpansionVarsPathProvider) },
        { TranspilerId.Cpp, typeof(CppExpansionVarsPathProvider) },
        { TranspilerId.C99, typeof(CExpansionVarsPathProvider) },
        { TranspilerId.CSharp, typeof(CSharpExpansionVarsPathProvider) },
        { TranspilerId.JavaScript, typeof(CSharpExpansionVarsPathProvider) },
        { TranspilerId.Java, typeof(CSharpExpansionVarsPathProvider) },
        { TranspilerId.Python, typeof(PythonExpansionVarsPathProvider) },
        { TranspilerId.TypeScript, typeof(CSharpExpansionVarsPathProvider) }
    };

    Dictionary<TranspilerId, Type> INAMEMANGLER_TYPES = new Dictionary<TranspilerId, Type>
    {
        { TranspilerId.Default, typeof(NameMangler) },
        { TranspilerId.Cpp, typeof(CamelCaseNameMangler) },
        { TranspilerId.C99, typeof(NameMangler) },
        { TranspilerId.CSharp, typeof(PascalFuncCamelVarNameMangler) },
        { TranspilerId.JavaScript, typeof(CamelCaseNameMangler) },
        { TranspilerId.Java, typeof(CamelCaseNameMangler) },
        { TranspilerId.Python, typeof(CamelCaseNameMangler) },
        { TranspilerId.TypeScript, typeof(CamelCaseNameMangler) }
    };

    Dictionary<AlgorithmId, Type> IGILALGO_TYPES = new Dictionary<AlgorithmId, Type>
    {
        { AlgorithmId.Default, typeof(AlgoBalanced2) },
        { AlgorithmId.Balanced1, typeof(AlgoBalanced1) },
        { AlgorithmId.Balanced2, typeof(AlgoBalanced2) }
    };

    Dictionary<TranspilerId, Type> IAUTOVARSPARSER_TYPES = new Dictionary<TranspilerId, Type>
    {
        { TranspilerId.Default, typeof(CLikeAutoVarsParser) },
        { TranspilerId.Cpp, typeof(CLikeAutoVarsParser) },
        { TranspilerId.C99, typeof(CLikeAutoVarsParser) },
        { TranspilerId.CSharp, typeof(CLikeAutoVarsParser) },
        { TranspilerId.JavaScript, typeof(JsAutoVarsParser) },
        { TranspilerId.Java, typeof(CLikeAutoVarsParser) },
        { TranspilerId.Python, typeof(PythonAutoVarsParser) },
        { TranspilerId.TypeScript, typeof(TypeScriptAutoVarsParser) }
    };

}

