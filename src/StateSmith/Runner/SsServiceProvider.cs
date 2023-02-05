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

public class SsServiceProvider
{
    private IHost? host;
    private readonly IHostBuilder hostBuilder;

    public SsServiceProvider()
    {
        hostBuilder = Host.CreateDefaultBuilder();
    }

    public static SsServiceProvider CreateDefault()
    {
        SsServiceProvider sp = new();
        sp.SetupAsDefault();
        return sp;
    }

    public void SetupAsDefault()
    {
        hostBuilder.ConfigureServices((services) =>
        {
            AddDefaultsForTesting(services);

            services.AddSingleton<CodeGenContext>();
            services.AddSingleton<SmTransformer, DefaultSmTransformer>();
            services.AddSingleton<Expander>();

            services.AddTransient<AutoExpandedVarsProcessor>();
            services.AddTransient<RenderConfigVerticesProcessor>();
            services.AddTransient<CodeGenRunner>();
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

    public void AddSingleton(IDiagramVerticesProvider diagramVerticesProvider)
    {
        ThrowIfAlreadyBuilt();
        hostBuilder.ConfigureServices(services => { services.AddSingleton(diagramVerticesProvider); });
    }

    public void AddSingleton(IStateMachineProvider obj)
    {
        ThrowIfAlreadyBuilt();
        hostBuilder.ConfigureServices(services => { services.AddSingleton(obj); });
    }

    /// <summary>
    /// Can only be done once. Limitation of lib.
    /// </summary>
    public void Build()
    {
        host = hostBuilder.Build(); // this will throw an exception if already built
    }

    private void ThrowIfAlreadyBuilt()
    {
        if (host != null)
        {
            throw new InvalidOperationException("Can't add after built");
        }
    }

    private static void AddDefaultsForTesting(IServiceCollection services)
    {
        services.AddSingleton(new DrawIoSettings());
        services.AddSingleton(new CNameMangler());
        services.AddSingleton(new CodeStyleSettings());
        services.AddSingleton<RenderConfigC>();
    }

    /// <summary>
    /// This class has implicit conversions that give some compile time type safety to <see cref="SsServiceProvider.GetServiceOrCreateInstance"/>.
    /// </summary>
    internal class ConvertableType
    {
        public IHost host;

        public ConvertableType(IHost host)
        {
            this.host = host;
        }

        public static implicit operator DrawIoToSmDiagramConverter(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<DrawIoToSmDiagramConverter>(me.host.Services);
        public static implicit operator DrawIoSettings(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<DrawIoSettings>(me.host.Services);
        public static implicit operator CNameMangler(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<CNameMangler>(me.host.Services);
        public static implicit operator SmTransformer(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<SmTransformer>(me.host.Services);
        public static implicit operator CodeGenRunner(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<CodeGenRunner>(me.host.Services);
        public static implicit operator RenderConfigC(ConvertableType me) => ActivatorUtilities.GetServiceOrCreateInstance<RenderConfigC>(me.host.Services);
    }

    /// <summary>
    /// Probably should only be used for test code at this point.
    /// </summary>
    /// <returns></returns>
    internal ConvertableType GetServiceOrCreateInstance()
    {
        return new ConvertableType(host.ThrowIfNull());
    }
}
