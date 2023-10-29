using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ViewComponentPreviewRCL.Services;

namespace ViewComponentPreviewRCL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUmbracoBlockViewComponents(this IServiceCollection serviceCollection)
    {
        var types = Assembly.GetEntryAssembly()
            ?.GetTypes().Where(x => !x.IsAbstract && x.IsClass && x.GetInterface(nameof(IUmbracoBlockComponentBase)) == typeof(IUmbracoBlockComponentBase));
        if (types != null)
        {
            foreach (var type in types)
            {
                serviceCollection.Add(new ServiceDescriptor(typeof(IUmbracoBlockComponentBase), type, ServiceLifetime.Transient));
            }
        }
        serviceCollection.AddTransient<UmbracoViewComponentService>();

        return serviceCollection;
    }
}