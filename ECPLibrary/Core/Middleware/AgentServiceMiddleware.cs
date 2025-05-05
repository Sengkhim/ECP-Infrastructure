using ECPLibrary.Services;

namespace ECPLibrary.Core.Middleware;

public static class AgentServiceMiddleware
{
    public static void UseAgents(this WebApplication application)
    {
        var service = application
            .Services
            .GetRequiredService<IAgentServiceHandler>();
        
        service.Handle().Wait();
    }
}