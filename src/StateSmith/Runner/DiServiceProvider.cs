using StateSmith.Input.DrawIo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using StateSmith.Output.C99BalancedCoder1;
using StateSmith.Input.Expansions;
using StateSmith.SmGraph;
using StateSmith.Output.UserConfig;
using StateSmith.Output;
using StateSmith.Common;

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// Dependency Injection Service Provider
/// </summary>
public class DiServiceProvider
{
    private IHost? host;
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
            AddDefaultsForTesting(services);

            services.AddSingleton(this); // todo_low remove. See https://github.com/StateSmith/StateSmith/issues/97
            services.AddSingleton<SmRunnerInternal>();
            services.AddSingleton<CodeGenContext>();
            services.AddSingleton<SmTransformer, StandardSmTransformer>();
            services.AddSingleton<Expander>();
            services.AddSingleton<InputSmBuilder>();
            services.AddSingleton<IConsolePrinter, ConsolePrinter>();
            services.AddSingleton<ExceptionPrinter>();

            services.AddSingleton<StateMachineProvider>();
            services.AddSingleton<IStateMachineProvider>((s) => s.GetService<StateMachineProvider>()!); // need to use lambda or else another object will be created

            services.AddSingleton<DiagramToSmConverter>();
            services.AddSingleton<IDiagramVerticesProvider>((s) => s.GetService<DiagramToSmConverter>()!); // need to use lambda or else another `DiagramToSmConverter` is created.

            services.AddTransient<AutoExpandedVarsProcessor>();
            services.AddTransient<RenderConfigVerticesProcessor>();
            services.AddTransient<ICodeGenRunner, CodeGenRunner>();
            services.AddTransient<MxCellsToSmDiagramConverter>();
            services.AddTransient<DrawIoToSmDiagramConverter>();
            services.AddTransient<VisualGroupingValidator>();
            services.AddTransient<DynamicVarsResolver>();
            services.AddTransient<ExpansionConfigReader>();
            services.AddTransient<CBuilder>();
            services.AddTransient<CHeaderBuilder>();
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

    public void AddSingletonT<TInterface, TImplementation>(TImplementation implementationObj) where TInterface : class   where TImplementation : TInterface
    {
        ThrowIfAlreadyBuilt();
        hostBuilder.ConfigureServices(services => { services.AddSingleton<TInterface>(implementationObj); });
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

    private static void AddDefaultsForTesting(IServiceCollection services)
    {
        services.AddSingleton(new DrawIoSettings());
        services.AddSingleton(new CNameMangler());
        services.AddSingleton(new CodeStyleSettings());
        services.AddSingleton<RenderConfigC>();
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
        public static implicit operator CNameMangler(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<CNameMangler>(me.host.Services);
        public static implicit operator SmTransformer(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<SmTransformer>(me.host.Services);
        public static implicit operator RenderConfigC(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<RenderConfigC>(me.host.Services);
        public static implicit operator InputSmBuilder(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<InputSmBuilder>(me.host.Services);
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
}
