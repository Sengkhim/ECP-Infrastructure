using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ECPLibrary.Extensions;

public static class HealthCheckEndpoints
{
    public static void UseHealthCheck(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;

                var payload = new
                {
                    status = report.Status.ToString(),
                    totalDuration  = report.TotalDuration,
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        data = e.Value.Data,
                        tags = e.Value.Tags,
                        duration = e.Value.Duration,
                        exception = e.Value.Exception?.Message
                    })
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(payload, options);
                await context.Response.WriteAsync(json);
            }
        });
    }
}