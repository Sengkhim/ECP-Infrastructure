
namespace ECPLibrary.Services;

/// <summary>
/// Defines methods for discovering service URLs, with optional load balancing support.
/// </summary>
public interface IEcpServiceDiscovery
{
    /// <summary>
    /// Retrieves the URL of a specific service endpoint with load balancing.
    /// </summary>
    /// <param name="serviceName">The name of the service to discover.</param>
    /// <param name="endpoint">The endpoint to access within the service.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the service URL with the specified endpoint, if found; otherwise, <c>null</c>.
    /// </returns>
    Task<string?> GetServiceUrlAsync(string serviceName, string endpoint);
}
