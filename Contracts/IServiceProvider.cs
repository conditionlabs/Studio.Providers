using Microsoft.AspNetCore.Builder;

namespace Studio.Providers.Contracts;

public interface IServiceProvider
{
    /// <summary>
    /// The application instance.
    /// </summary>
    WebApplication app { get; set; }
}
