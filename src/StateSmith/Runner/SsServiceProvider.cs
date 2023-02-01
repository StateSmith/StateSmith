using StateSmith.Input.DrawIo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using StateSmith.output.C99BalancedCoder1;

#nullable enable

namespace StateSmith.Runner;

public class SsServiceProvider
{
    private readonly IHost host;

    public SsServiceProvider(Action<HostBuilderContext, IServiceCollection>? preConfigAction = null, Action<HostBuilderContext, IServiceCollection>? postConfigAction = null)
    {
        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                preConfigAction?.Invoke(context, services);

                services.AddSingleton(new DrawIoSettings());
                services.AddSingleton(new CNameMangler());
                services.AddSingleton<SmTransformer, DefaultSmTransformer>();

                services.AddTransient<MxCellsToSmDiagramConverter>();
                services.AddTransient<DrawIoToSmDiagramConverter>();
                services.AddTransient<VisualGroupingValidator>();

                postConfigAction?.Invoke(context, services);
            })
            .Build();
    }

    /// <summary>
    /// This class has implicit conversions that give some compile time type safety to <see cref="SsServiceProvider.GetServiceOrCreateInstance"/>.
    /// </summary>
    public class ConvertableType
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
    }

    /// <summary>
    /// Probably should only be used for test code at this point.
    /// </summary>
    /// <returns></returns>
    public ConvertableType GetServiceOrCreateInstance()
    {
        return new ConvertableType(host);
    }
}
