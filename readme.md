# Service Providers

- [Introduction](#introduction)
- [Installation](#installation)
- [Writing Service Providers](#writing-service-providers)
  - [The Register Method](#the-register-method)
  - [The Boot Method](#the-boot-method)
  - [Boot Method Dependency Injection](#boot-method-dependency-injection)
- [Registering Providers](#registering-providers)
- [Executing Providers](#executing-providers)

## Introduction

Service providers are the central place for configuring an application's services and bootstrapping its components. All service providers extend the `Studio.Providers.ServiceProvider` base class and implement a two-step initialization process through the `Register` and `Boot` methods.

The `Register` method is exclusively responsible for binding services into the service container. Within this method, nothing else should be done besides registering service bindings. Otherwise, services bound by providers that have not loaded yet may accidentally be used.

After all providers have registered their services, the `Boot` method is called on each provider. At this point, the application has been fully constructed and all services are available, allowing providers to configure middleware, routing, and other application features.

## Installation

Studio.Providers may be installed via the .NET CLI:

```csharp
dotnet package add Studio.Providers
```

## Writing Service Providers

All service providers extend the `Studio.Providers.ServiceProvider` class. Most providers contain a `Register` and a `Boot` method. The `Register` method is called first, followed by the `Boot` method once all providers have been registered.

### The Register Method

Within the `Register` method, services should only be bound into the `IServiceCollection`. The `Register` method is called before the application is built, meaning the `WebApplication` instance does not exist yet and middleware or routing cannot be configured at this stage:

```csharp
namespace App.Providers;

public class ViewServiceProvider : Studio.Providers.ServiceProvider
{
    /// <summary>
    /// Register any application services.
    /// </summary>
    public void Register(IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
    }
}
```

### The Boot Method

The `Boot` method is called after all services have been registered and the application has been built. This method should be used to configure middleware, routing, and perform any setup that requires access to registered services. The `app` property provides access to the `WebApplication` instance:

```csharp
namespace App.Providers;

public class ExceptionServiceProvider : Studio.Providers.ServiceProvider
{
    /// <summary>
    /// Bootstrap any application services.
    /// </summary>
    public void Boot()
    {
        if (!this.app.Environment.IsDevelopment())
        {
            this.app.UseExceptionHandler("/Error", createScopeForErrors: true);
            this.app.UseHsts();
        }
    }
}
```

### Boot Method Dependency Injection

Dependencies may be added as parameters in the `Boot` method's signature. The service container will automatically try to inject any dependencies the `Boot` method needs:

```csharp
using App.Factories;

public void Boot(ResponseFactory response)
{
    response.Make(() => 
    {
        // ...
    });
}
```

## Registering Providers

Providers are registered in the `Program.cs` file using the `AddProviders` extension method. The order in which providers are registered determines the order in which they are executed. This is important when providers depend on services registered by other providers.

For example, if `AuthServiceProvider` registers authentication services that `RouteServiceProvider` needs during its boot phase, then `AuthServiceProvider` must be registered before `RouteServiceProvider`:

```csharp
using Studio.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProviders(providers =>
{
    providers
        .Add<AppServiceProvider>()
        .Add<AuthServiceProvider>()
        .Add<RouteServiceProvider>()
        .Add<ViewServiceProvider>();
});
```

In this example, `AuthServiceProvider` registers authentication services in its `Register` method. The `RouteServiceProvider`, which is registered after `AuthServiceProvider`, can safely use these authentication services in its `Boot` method to configure protected routes.

## Executing Providers

After building the application, the `UseProviders` method must be called to execute the boot phase. This invokes the `Boot` method on all registered providers in the order they were registered:

```csharp
var app = builder.Build();

app.UseProviders();

app.Run();
```

## License

Studio.Providers is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
