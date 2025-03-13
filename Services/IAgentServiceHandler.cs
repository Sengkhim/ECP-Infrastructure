namespace ECPLibrary.Services;

/// <summary>
/// Interface for handling agent service registration with Consul.
/// Provides methods to register and manage the service in Consul.
/// </summary>
public interface IAgentServiceHandler
{
    /// <summary>
    /// Registers a service with Consul using the provided agent service check options.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of the service registration.</returns>
    Task Handle();
}
