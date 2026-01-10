using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Studio.Providers;

public static class ServiceProviderExtension
{
    /// <summary>
    /// Register all service providers.
    /// </summary>
    public static IServiceCollection AddProviders(this IServiceCollection services, Action<ServiceProviderManager> configure)
    {
        var manager = new ServiceProviderManager(services);
        configure(manager);

        services.AddSingleton(manager);

        manager.RegisterAll();

        return services;
    }

    /// <summary>
    /// Bootstrap all service providers.
    /// </summary>
    public static IApplicationBuilder UseProviders(this WebApplication app)
    {
        var manager = app.Services.GetRequiredService<ServiceProviderManager>();
        manager.BootAll(app);
        return app;
    }
}
