using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ECPLibrary.Extensions;

public static class ApplicationExtensions
{
    public static void UseHealthCheck(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        data = e.Value.Data,
                        tages = e.Value.Tags,
                        duration = e.Value.Duration,
                        exception = e.Value.Exception
                    })
                });
                await context.Response.WriteAsync(result);
            }
        });
    }
}