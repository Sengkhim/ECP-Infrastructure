using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Consul;
using ECPLibrary.Core.Attributes;
using ECPLibrary.Implement;
using ECPLibrary.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECPLibrary.Extensions;

public static class ServiceDiscoveryExtension
{
    private const string Localhost = "127.0.0.1";
    private const string ConsulHost = "http://localhost:8500";
    private static readonly string[] Tags = ["ready"];

    /// <summary>
    /// Retrieves the first available IPv4 address of the current machine. 
    /// If no valid IPv4 address is found, returns the specified prefix.
    /// </summary>
    /// <param name="prefix">The default value to return if no IPv4 address is found.</param>
    /// <returns>The first available IPv4 address as a string, or the provided prefix if no address is found.</returns>
    public static string GetDns(string prefix)
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
            ?.ToString() ?? prefix;
    }
    
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

    /// <summary>
    /// Constructs a base URL using the configured protocol and the first available IPv4 address of the host.
    /// </summary>
    /// <param name="config">The application configuration instance.</param>
    /// <param name="prefix">A fallback value if the DNS resolution fails.</param>
    /// <returns>The fully constructed base URL (e.g., "http://192.168.1.10").</returns>
    public static string Url(this IConfiguration config, string prefix = Localhost) 
        => $"{config.GetProtocol()}{GetDns(prefix)}:{config.EnsurePort()}";

    /// <summary>
    /// Retrieves the deployment type from the application configuration.
    /// </summary>
    /// <param name="config">The application configuration instance.</param>
    /// <returns>
    /// <c>true</c> if the application is deployed in a Docker environment; otherwise, <c>false</c>.
    /// </returns>
    public static bool GetDeployType(this IConfiguration config)
        => config.GetValue<bool>("DeployType:Docker");
    
    /// <summary>
    /// Configures the web host URL based on the deployment type.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> used to configure the application.</param>
    public static void HostUrl(this WebApplicationBuilder builder)
    {
        var host = builder.Configuration.Url();
        builder.WebHost.UseUrls(host);
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
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: Tags);

        services.AddSingleton<IConsulClient>(new ConsulClient(options =>
        {
            var port = 8500;  
            var portString = Environment.GetEnvironmentVariable("CONSUL_PORT");

            if (!string.IsNullOrEmpty(portString) && int.TryParse(portString, out var parsedPort))
                port = parsedPort;
            
            var host = Environment.GetEnvironmentVariable("CONSUL_HOST") ?? "host.docker.internal";
            
            options.Address = new Uri($"http://{host}:{port}");
        }));
        
        services.EnsureAgentRegister();
        
        services.AddScoped<IEcpServiceDiscovery, EcpServiceDiscovery>();
    }

    /// <summary>
    /// Registers all classes annotated with <see cref="UseAgentAttribute"/> as singleton services.
    /// </summary>
    /// <param name="services">The IServiceCollection instance.</param>
    private static void EnsureAgentRegister(this IServiceCollection services)
    {
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.GetCustomAttribute<UseAgentAttribute>() != null && typeof(IAgentServiceHandler).IsAssignableFrom(type))
            .ToList()
            .ForEach(type => services.AddSingleton(typeof(IAgentServiceHandler), type));
    }
}