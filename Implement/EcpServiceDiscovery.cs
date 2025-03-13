using Consul;
using ECPLibrary.Extensions;
using ECPLibrary.Services;

namespace ECPLibrary.Implement;

public class EcpServiceDiscovery(
    IConsulClient consulClient, 
    IConfiguration configuration) : IEcpServiceDiscovery
{
    public async Task<string?> GetServiceUrlAsync(string serviceName, string endpoint)
    {
        var services = await consulClient.Agent.Services();
        var serviceEntries = services.Response
            .Values
            .Where(s => s.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (serviceEntries.Count == 0) return null;

        var service = serviceEntries[Random.Shared.Next(serviceEntries.Count)];
        var protocol = configuration.GetProtocol().Trim();
        var address = service.Address.Trim();
    
        return $"{protocol}{address}:{service.Port}/{endpoint.Trim()}";
    }
}