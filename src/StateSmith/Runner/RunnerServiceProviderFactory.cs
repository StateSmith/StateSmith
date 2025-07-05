
#nullable enable

using StateSmith.Input.DrawIo;
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

/// <summary>
/// Provides a default implementation of <see cref="RunnerServiceProviderFactory"/> that sets up a service provider with common services used by StateSmith.
/// This builder can be used to configure additional services or override existing ones.
/// </summary>
public class RunnerServiceProviderFactory 
{
    public static IServiceProvider CreateDefault(Action<IServiceCollection>? serviceOverrides = null)
    {
        ServiceCollection services = new();

        // RunnerContext
        services.AddSingleton<RunnerContext>();
        services.AddSingleton<RunnerSettings>((sp) => sp.GetRequiredService<RunnerContext>().runnerSettings);

        // RunnerSettings
        services.AddSingleton<DrawIoSettings>((sp) => sp.GetRequiredService<RunnerSettings>().drawIoSettings);
        services.AddSingleton<CodeStyleSettings>((sp) => sp.GetRequiredService<RunnerSettings>().style);
        services.AddSingleton<SmDesignDescriberSettings>((sp) => sp.GetRequiredService<RunnerSettings>().smDesignDescriber);
        services.AddSingleton<AlgoBalanced1Settings>((sp) => sp.GetRequiredService<RunnerSettings>().algoBalanced1);

        // RenderConfig
        // TODO can I generate RenderConfigAllVars from iRenderConfig?
        services.AddSingleton<IRenderConfig>((sp) => sp.GetRequiredService<RunnerContext>().renderConfig);
        services.AddSingleton((sp) => new ExpansionConfigReaderObjectProvider(sp.GetRequiredService<IRenderConfig>()));
        services.AddSingleton<RenderConfigAllVars>((sp) => sp.GetRequiredService<RunnerContext>().renderConfigAllVars);
        services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().Base);
        services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().C);
        services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().Cpp);
        services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().CSharp);
        services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().JavaScript);
        services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().Java);
        services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().Python);
        services.AddSingleton(sp => sp.GetRequiredService<RenderConfigAllVars>().TypeScript);

        services.AddSingleton<SmRunnerInternal>();
        services.AddSingleton<SmTransformer, StandardSmTransformer>();
        services.AddSingleton<IExpander, Expander>();
        services.AddSingleton<InputSmBuilder>();
        services.AddSingleton<IConsolePrinter, ConsolePrinter>();
        services.AddSingleton<ExceptionPrinter>();
        services.AddSingleton<ICodeFileWriter, CodeFileWriter>();

        services.AddSingleton<StateMachineProvider>();
        services.AddSingleton<IStateMachineProvider>((s) => s.GetRequiredService<StateMachineProvider>());
        services.AddSingleton<DiagramFilePathProvider>();
        services.AddSingleton<SmFileNameProcessor>();

        services.AddSingleton<DiagramToSmConverter>();
        services.AddSingleton<IDiagramVerticesProvider>((s) => s.GetRequiredService<DiagramToSmConverter>());
        services.AddSingleton<AlgoTranspilerCustomizer>();
        services.AddSingleton<IAlgoStateIdToString, AlgoStateIdToString>();
        services.AddSingleton<IAlgoEventIdToString, AlgoEventIdToString>();
        services.AddSingleton<GilToC99Customizer>();
        services.AddSingleton<IGilToC99Customizer>((s) => s.GetRequiredService<GilToC99Customizer>());
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

        services.AddSingleton<OutputInfo>();
        services.AddSingleton<IOutputInfo>((s) => s.GetRequiredService<OutputInfo>());
        services.AddSingleton<FilePathPrinter>((sp) => new FilePathPrinter(sp.GetRequiredService<RunnerSettings>().filePathPrintBase ?? ""));

        services.AddSingleton<ExpansionsPrep>();

        // Merge the overrides into the service collection.
        serviceOverrides?.Invoke(services);

        return services.BuildServiceProvider();
    }


    // Helper to resolve a service by id from a type map
    private static TService ResolveServiceFromRunnerSettings<TService, TId>(IServiceProvider sp, Func<RunnerSettings, TId> idSelector, IReadOnlyDictionary<TId, Type> typeMap)
    {
        var settings = sp.GetRequiredService<RunnerSettings>();
        var id = idSelector(settings);
        Type t = typeMap[id].ThrowIfNull($"{id?.GetType()} '{id}' is not supported.");
        return (TService)ActivatorUtilities.GetServiceOrCreateInstance(sp, t);
    }


    static Dictionary<TranspilerId, Type> IGILTRANSPILER_TYPES = new Dictionary<TranspilerId, Type> {
        { TranspilerId.Default, typeof(GilToC99)},
        { TranspilerId.Cpp, typeof(GilToCpp)},
        { TranspilerId.C99, typeof(GilToC99)},
        { TranspilerId.CSharp, typeof(GilToCSharp)},
        { TranspilerId.JavaScript, typeof(GilToJavaScript)},
        { TranspilerId.Java, typeof(GilToJava)},
        { TranspilerId.Python, typeof(GilToPython)},
        { TranspilerId.TypeScript, typeof(GilToTypeScript)}
    };

    static Dictionary<TranspilerId, Type> IEXPANSIONVARSPATHPROVIDER_TYPES = new Dictionary<TranspilerId, Type>
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

    static Dictionary<TranspilerId, Type> INAMEMANGLER_TYPES = new Dictionary<TranspilerId, Type>
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

    static Dictionary<AlgorithmId, Type> IGILALGO_TYPES = new Dictionary<AlgorithmId, Type>
    {
        { AlgorithmId.Default, typeof(AlgoBalanced2) },
        { AlgorithmId.Balanced1, typeof(AlgoBalanced1) },
        { AlgorithmId.Balanced2, typeof(AlgoBalanced2) }
    };

    static Dictionary<TranspilerId, Type> IAUTOVARSPARSER_TYPES = new Dictionary<TranspilerId, Type>
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

