using ECPLibrary.Core.Middleware;
using ECPLibrary.Extensions;
using ECPLibrary.tests;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCoreServiceDiscovery();
builder.Services
    .AddCoreEcpLibrary<InventoryDbContext>(builder.Configuration, "InventoryDb");

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Running"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseAgents();
app.UseHealthCheck();
app.Run();