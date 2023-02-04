using StateSmith.Input.DrawIo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using StateSmith.Output.C99BalancedCoder1;
using StateSmith.Input.Expansions;
using StateSmith.SmGraph;
using StateSmith.Output.UserConfig;
using StateSmith.Output;

#nullable enable

namespace StateSmith.Runner;

public class SsServiceProvider
{
    private readonly IHost host;

    /// <summary>
    /// The StateMachine cannot be known at startup so we can't register it normally with the Service Provider.
    /// </summary>
    public Func<StateMachine> SmGetter = () => throw new NotImplementedException();

    /// <summary>
    /// This info cannot be known at startup so we can't register it normally with the Service Provider.
    /// </summary>
    public Func<IDiagramVerticesProvider> IDiagramVerticesProviderGetter = () => throw new NotImplementedException();

    public SsServiceProvider(Action<HostBuilderContext, IServiceCollection>? preConfigAction = null, Action<HostBuilderContext, IServiceCollection>? postConfigAction = null)
    {
        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                preConfigAction?.Invoke(context, services);

                AddDefaultsForTesting(services);

                services.AddSingleton<CodeGenContext>();
                services.AddSingleton<SmTransformer, DefaultSmTransformer>();
                services.AddSingleton<Expander>();
                services.AddSingleton<IDiagramVerticesProvider>((sp) => IDiagramVerticesProviderGetter() );

                services.AddTransient<RenderConfigVerticesProcessor>();
                services.AddTransient<CodeGenRunner>();
                services.AddTransient<MxCellsToSmDiagramConverter>();
                services.AddTransient<DrawIoToSmDiagramConverter>();
                services.AddTransient<VisualGroupingValidator>();
                services.AddTransient<DynamicVarsResolver>();
                services.AddTransient<ConfigReader>();
                services.AddTransient<CBuilder>();
                services.AddTransient<CHeaderBuilder>();

                services.AddTransient<StateMachine>(sp => SmGetter());

                postConfigAction?.Invoke(context, services);
            })
            .Build();
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
    }

    /// <summary>
    /// Probably should only be used for test code at this point.
    /// </summary>
    /// <returns></returns>
    internal ConvertableType GetServiceOrCreateInstance()
    {
        return new ConvertableType(host);
    }
}
