using System.Net;
using Consul;
using ECPLibrary.Core.Abstraction;
using ECPLibrary.Core.Attributes;
using ECPLibrary.Extensions;

namespace ECPLibrary.Implement;

[UseAgent]
public sealed class AgentServiceRegisterHandler(IHost host) : AgentServiceContext(host)
{
    private readonly IHost _host = host;

    protected override AgentServiceRegistration Register()
    {
        var config = _host.Services.GetRequiredService<IConfiguration>();
        var serviceId = Environment.GetEnvironmentVariable("SERVICE_ID") ?? Dns.GetHostName();
        var serviceName = Environment.GetEnvironmentVariable("APPLICATION_NAME") ?? "ecp-service";
        var port = config.EnsurePort();
        const string host = "host.docker.internal";

        return new AgentServiceRegistration
        {
            ID = serviceId,
            Name = serviceName,
            Address = host,
            Port = port,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{host}:{port}/health",
                Interval = TimeSpan.FromSeconds(10), 
                Timeout = TimeSpan.FromSeconds(5),   
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };
    }
}
