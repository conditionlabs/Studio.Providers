using Microsoft.AspNetCore.Builder;

namespace Studio.Providers;

public abstract class ServiceProvider : Contracts.IServiceProvider
{
    /// <summary>
    /// The application instance.
    /// </summary>
    public WebApplication app { get; set; } = null!;
}
