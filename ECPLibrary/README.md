[//]: # (1- dotnet pack)
[//]: # (2- dotnet nuget push bin/Release/SK.ECP.Infrastructure.1.0.6.nupkg --api-key oy2esfogp2bbhh2ako4avdr2hepqrnfenjtx3dhxivdh3a --source https://api.nuget.org/v3/index.json)

## 📦 Features

- Exited database identity context 
- Service registry, Service discovery
- Repository and unit of work pattern

###  Database identity context
To create database have example below:

```
public class FakeDbContext(DbContextOptions<FakeDbContext> options,
    IConfigurationModeling modeling)
    : EcpDatabase<FakeDbContext>(options, modeling) {
    
    }
````

### Database register
Then register to FakeDbContext to program.cs have some option

````
Option 1: Register database service only:

var config = builder.Configuration;
builder.Services.AddDatabase<FakeDbContext>(config, "Xunit");

````

````
Option 2: Register some service:
- Database service
- Configuration modeling service
- Unit of work serevice

var config = builder.Configuration;
builder.Services.AddCoreEcpLibrary<FakeDbContext>(config, "Xunit")
````

````
Option 3: Register some service:
- Database service ( provide option to config own database context)
- Configuration modeling service
- Unit of work serevice

var config = builder.Configuration;

builder.Services.AddCoreEcpLibrary<FakeDbContext>((svc) =>
         {
             svc.AddDbContext<IEcpDatabase, FakeDbContext>(opt =>
                 opt.UseNpgsql("Host=localhost;Database=xunit;Username=postgres;Password=9090"));
         });
````


### Core Service Discovery context

````
To create service discovery
- Create a class then example from AgentServiceContext
- Use attribute [UseAgent] on class 

Example:

[UseAgent]
public class AgentServiceRegisterHandler(IHost host) : AgentServiceContext(host)
{
    private readonly IHost _agentHost = host;

    protected override AgentServiceRegistration Register()
    {
        var config = _agentHost.Services.GetRequiredService<IConfiguration>();
        var serviceId = Environment.GetEnvironmentVariable("SERVICE_ID") ?? Dns.GetHostName();
        var serviceName = Environment.GetEnvironmentVariable("APPLICATION_NAME") ?? "ecp-service";
        var port = config.EnsurePort();
        const string address = "host.docker.internal";

        return new AgentServiceRegistration
        {
            ID = serviceId,
            Name = serviceName,
            Address = address,
            Port = port,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{address}:{port}/health",
                Interval = TimeSpan.FromSeconds(10), 
                Timeout = TimeSpan.FromSeconds(5),   
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };
    }
}

````

### Service Discovery register
````
Register AddCoreServiceDiscovery in program.cs

builder.Services.AddCoreServiceDiscovery();

Use this middleware to mapping service discovery
app.UseAgents();
````