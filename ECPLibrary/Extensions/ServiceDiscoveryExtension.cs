using System.Reflection;
using Consul;
using ECPLibrary.Core.Attributes;
using ECPLibrary.Implement;
using ECPLibrary.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECPLibrary.Extensions;

public static class ServiceDiscoveryExtension
{
    private const string Localhost = "localhost";
    private static readonly string[] Tags = ["ready"];
    
    /// <summary>
    /// Retrieves the protocol (HTTP or HTTPS) based on the ASP.NET Core configuration.
    /// </summary>
    /// <param name="config">The application configuration instance.</param>
    /// <returns>
    /// Returns "https://" if the `ASPNETCORE_URLS` setting starts with "https://", otherwise returns "http://".
    /// </returns>
    public static string GetProtocol(this IConfiguration config)
    {
        var url = config["ASPNETCORE_URLS"];
        return !string.IsNullOrEmpty(url) && url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? "https://"
            : "http://";
    }

    /// <summary>
    /// Retrieves the port number from the ASP.NET Core configuration.
    /// </summary>
    /// <param name="config">The application configuration instance.</param>
    /// <returns>
    /// The extracted port number from the `ASPNETCORE_URLS` setting, or the default port (8080) if unavailable.
    /// </returns>
    private static int GetPort(this IConfiguration config)
    {
        var url = config["ASPNETCORE_URLS"];
        if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Port;
        }
        return 80;
    }

    public static int EnsurePort(this IConfiguration configuration)
    {
        var port = Environment.GetEnvironmentVariable("ASPNETCORE_PORT");
        return port is not null ? int.Parse(port) : configuration.GetPort();
    }
    
    /// <summary>
    /// Adds core service discovery and health check functionality to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    public static void AddCoreServiceDiscovery(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("SELF", () => HealthCheckResult.Healthy(), tags: Tags);

        services.AddSingleton<IConsulClient>(new ConsulClient(options =>
        {
            var port = 8500;  
            var portString = Environment.GetEnvironmentVariable("CONSUL_PORT");

            if (!string.IsNullOrEmpty(portString) && int.TryParse(portString, out var parsedPort))
                port = parsedPort;
            
            var host = Environment.GetEnvironmentVariable("CONSUL_HOST") ?? Localhost;
            
            options.Address = new Uri($"http://{host}:{port}");
        }));
        
        services.EnsureAgentRegister();
        
        services.AddScoped<IEcpServiceDiscovery, EcpServiceDiscovery>();
    }

    /// <summary>
    /// Registers all classes annotated with <see cref="UseAgentAttribute"/> as singleton services.
    /// </summary>
    /// <param name="services">The IServiceCollection instance.</param>
    public static void EnsureAgentRegister(this IServiceCollection services)
    {
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.GetCustomAttribute<UseAgentAttribute>() != null && typeof(IAgentServiceHandler).IsAssignableFrom(type))
            .ToList()
            .ForEach(type => services.AddSingleton(typeof(IAgentServiceHandler), type));
    }
}