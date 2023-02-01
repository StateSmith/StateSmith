using StateSmith.Input.DrawIo;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

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
