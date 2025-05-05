using Consul;

namespace ECPLibrary.Core.Configuration;

/// <summary>
/// Abstract record for holding service registration options for an agent.
/// Contains essential information like the prefix ID, instance name, service check details, and the container name.
/// </summary>
public abstract record AgentServiceCheckOption
{
    /// <summary>
    /// Gets the prefix ID for the service instance.
    /// </summary>
    public string PrefixId { get; }

    /// <summary>
    /// Gets the name of the service instance.
    /// </summary>
    public string InstanceName { get; }

    /// <summary>
    /// Gets the health check configuration for the agent service.
    /// </summary>
    public AgentServiceCheck Check { get; }

    /// <summary>
    /// Gets the name of the container associated with the service.
    /// </summary>
    public string ContainerName { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentServiceCheckOption"/> record.
    /// </summary>
    /// <param name="prefixId">The prefix ID for the service instance.</param>
    /// <param name="instanceName">The name of the service instance.</param>
    /// <param name="check">The health check configuration for the agent service.</param>
    /// <param name="containerName">The name of the container associated with the service.</param>
    protected AgentServiceCheckOption(string prefixId, string instanceName, AgentServiceCheck check, string containerName)
    {
        PrefixId = prefixId;
        InstanceName = instanceName;
        Check = check;
        ContainerName = containerName;
    }
}
