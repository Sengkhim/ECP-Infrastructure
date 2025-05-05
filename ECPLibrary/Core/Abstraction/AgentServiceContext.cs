using System.Diagnostics;
using Consul;
using ECPLibrary.Services;

namespace ECPLibrary.Core.Abstraction;

/// <summary>
/// Abstract class representing the context for an agent service in a distributed system.
/// This class provides a mechanism to register services with Consul using dependency injection.
/// </summary>
/// <param name="host">The host instance that provides access to the application's service container.</param>
public abstract class AgentServiceContext(IHost host) : IAgentServiceHandler
{
    /// <summary>
    /// Represents the application host providing access to services.
    /// Throws <see cref="ArgumentNullException"/> if the host is null.
    /// </summary>
    private readonly IHost _host = host ?? throw new ArgumentNullException(nameof(host));

    /// <summary>
    /// Registers the agent service.
    /// This method must be implemented by derived classes to provide service registration details.
    /// </summary>
    /// <returns>An instance of <see cref="AgentServiceRegistration"/> representing the service details.</returns>
    protected abstract AgentServiceRegistration Register();

    /// <summary>
    /// Handles the agent service registration process asynchronously.
    /// It retrieves an <see cref="IConsulClient"/> instance and registers the service with Consul.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task Handle()
    {
        using var scope = _host.Services.CreateScope();

        var client = scope.ServiceProvider.GetRequiredService<IConsulClient>();

        Debug.Assert(client != null, "IConsulClient is not registered!");

        var serviceInstance = Register();
        
        var appLifetime = _host.Services.GetRequiredService<IHostApplicationLifetime>();
        
        try
        {
            appLifetime
                .ApplicationStarted
                .Register(() => client.Agent.ServiceRegister(serviceInstance).Wait());
          
            appLifetime
                .ApplicationStopping
                .Register(() => client.Agent.ServiceDeregister(serviceInstance.ID).Wait());
            
            return Task.FromResult(Task.CompletedTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while registering service {serviceInstance.ID}: {ex.Message}");
            Debug.Assert(false, $"Failed to register service {serviceInstance.ID}: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}