using System.Reflection;
using Consul;
using ECPLibrary.Extensions;
using ECPLibrary.Implement;
using ECPLibrary.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ECPLibrary.Tests.Extensions;

public class CoreServiceDiscoveryExtensionTests
{
    private readonly Mock<IHostApplicationLifetime> _appLifetimeMock = new();
    private readonly Mock<IAgentEndpoint> _agentMock = new();
    private readonly Mock<IConsulClient> _mockConsul = new();
    private readonly Mock<IHost> _mockHost = new();
    private readonly Mock<IConfiguration> _mockConfiguration = new();
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly CancellationTokenSource _startedSource = new();
    private readonly CancellationTokenSource _stoppingSource = new();

    public CoreServiceDiscoveryExtensionTests()
    {
        _services.AddSingleton(_mockHost.Object);
        _services.AddSingleton(_mockConfiguration.Object);
    }
    
    [Fact]
    public void AddCoreServiceDiscovery_Registers_Consul_And_Agents()
    {
        // Act
        _services.AddCoreServiceDiscovery();
        
        var provider = _services.BuildServiceProvider();

        // Assert
        var consulClient = provider.GetService<IConsulClient>();
        var serviceDiscovery = provider.GetRequiredService<IEcpServiceDiscovery>();
        var agentServiceHandler = provider.GetService<IAgentServiceHandler>();
        
        Assert.NotNull(consulClient);
        Assert.IsAssignableFrom<IConsulClient>(consulClient);
        
        Assert.NotNull(consulClient);
        Assert.IsAssignableFrom<IEcpServiceDiscovery>(serviceDiscovery);
        
        Assert.NotNull(agentServiceHandler);
        Assert.IsAssignableFrom<IAgentServiceHandler>(agentServiceHandler);
    }
    
    [Fact]
    public void EnsureAgentRegister_Should_Register_Types_With_UseAgentAttribute_Without_Invoke()
    {
        // Act
        _services.EnsureAgentRegister();
        var provider = _services.BuildServiceProvider();

        // Assert
        var agentService = provider.GetRequiredService<IAgentServiceHandler>();
        
        Assert.NotNull(agentService);
        Assert.IsType<AgentServiceRegisterHandler>(agentService);
    }
    
    [Fact]
    public async Task EnsureAgentRegister_Should_Register_Types_With_UseAgentAttribute_And_Invoke()
    {

        // Mock IAgentEndpoint
        _agentMock
            .Setup(a => a.ServiceRegister(
                It.IsAny<AgentServiceRegistration>(),
                It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new WriteResult()));
        
        _agentMock
            .Setup(a => a.ServiceDeregister(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new WriteResult()));

        // Consul client returns that agent
        _mockConsul.Setup(c => c.Agent).Returns(_agentMock.Object);
        _services.AddSingleton(_mockConsul.Object);

        // Mock application lifetime
        _appLifetimeMock
            .Setup(a => a.ApplicationStarted)
            .Returns(_startedSource.Token);
        
        _appLifetimeMock
            .Setup(a => a.ApplicationStopping)
            .Returns(_stoppingSource.Token);
        
        _services.AddSingleton(_appLifetimeMock.Object);

        // Auto‑register all [UseAgent] handlers
        _services.EnsureAgentRegister();

        // Build the provider and fake host
        var provider = _services.BuildServiceProvider();
        _mockHost.Setup(h => h.Services).Returns(provider);

        // Act: instantiate and run the handler
        var handler = new AgentServiceRegisterHandler(_mockHost.Object);
        Assert.NotNull(handler);  
        Assert.IsType<AgentServiceRegisterHandler>(handler);

        // registers the callbacks, but doesn't yet invoke them
        await handler.Handle();

        // Trigger the callbacks by cancelling the tokens
        await _startedSource.CancelAsync();
        await _stoppingSource.CancelAsync();

        // Assert: the agent endpoint methods were called exactly once
        _agentMock.Verify(a => 
            a.ServiceRegister(It.IsAny<AgentServiceRegistration>(), It.IsAny<CancellationToken>()), 
            Times.Once);

        _agentMock.Verify(a => 
            a.ServiceDeregister(It.IsAny<string>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    
    [Fact]
    public void AgentServiceRegisterHandler_Should_Create_Valid_ServiceRegistration()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["Kestrel:Endpoints:Http:Url"] = "http://localhost:5000" }!)
            .Build();
        
        _services.AddSingleton<IConfiguration>(config);
        
        var host = new HostBuilder()
            .ConfigureServices(s => s.AddSingleton<IConfiguration>(config))
            .Build();
        
        var handler = new AgentServiceRegisterHandler(host);

        // Act
        var registration = handler
            .GetType()
            .GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(handler, null) as AgentServiceRegistration;

        // Assert
        Assert.NotNull(registration);
        Assert.NotEmpty(registration.ID);
        Assert.NotEmpty(registration.Name);
        Assert.Equal("host.docker.internal", registration.Address);
        Assert.NotNull(registration.Check);
        Assert.Contains("health", registration.Check.HTTP);
    }
    
}