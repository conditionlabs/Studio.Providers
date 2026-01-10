using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Studio.Providers;

public class ServiceProviderManager
{
    /// <summary>
    /// The application instance.
    /// </summary>
    private WebApplication? app;

    /// <summary>
    /// The collection of services.
    /// </summary>
    private readonly IServiceCollection services;

    /// <summary>
    /// The collection of service providers.
    /// </summary>
    private readonly List<Contracts.IServiceProvider> providers;

    /// <summary>
    /// Create a new ServiceProviderManager instance.
    /// </summary>
    public ServiceProviderManager(IServiceCollection services)
    {
        this.services = services;
        this.providers = new();
    }

    /// <summary>
    /// Register a service provider.
    /// </summary>
    public ServiceProviderManager Add(Contracts.IServiceProvider serviceProvider)
    {
        this.providers.Add(serviceProvider);
        return this;
    }

    /// <summary>
    /// Register a service provider.
    /// </summary>
    public ServiceProviderManager Add<T>() where T : Contracts.IServiceProvider, new()
    {
        this.providers.Add(new T());
        return this;
    }

    /// <summary>
    /// Bootstrap all service providers.
    /// </summary>
    public void BootAll(WebApplication app)
    {
        this.app = app;

        foreach (var provider in this.providers)
        {
            provider.app = this.app;
            BootProvider(provider);
        }
    }

    /// <summary>
    /// Boot a single service provider using reflection.
    /// </summary>
    private void BootProvider(Contracts.IServiceProvider provider)
    {
        var bootMethod = provider.GetType().GetMethod("Boot");
        if (bootMethod == null) return;

        var parameters = ResolveMethodParameters(bootMethod);
        bootMethod.Invoke(provider, parameters);
    }

    /// <summary>
    /// Resolve all parameters for a method via dependency injection.
    /// </summary>
    private object?[] ResolveMethodParameters(System.Reflection.MethodInfo method)
    {
        var parameters = method.GetParameters();
        var parameterValues = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            parameterValues[i] = ResolveParameter(parameters[i].ParameterType);
        }

        return parameterValues;
    }

    /// <summary>
    /// Resolve a single parameter type from the service provider.
    /// </summary>
    private object? ResolveParameter(Type paramType)
    {
        try
        {
            return this.app!.Services.GetRequiredService(paramType);
        }
        catch
        {
            return paramType.IsValueType ? Activator.CreateInstance(paramType) : null;
        }
    }

    /// <summary>
    /// Register all service providers.
    /// </summary>
    public void RegisterAll()
    {
        foreach (var provider in this.providers)
        {
            RegisterProvider(provider);
        }
    }

    /// <summary>
    /// Register a single service provider.
    /// </summary>
    private void RegisterProvider(Contracts.IServiceProvider provider)
    {
        var registerMethod = provider.GetType().GetMethod("Register");
        if (registerMethod == null) return;

        registerMethod.Invoke(provider, [this.services]);
    }
}
