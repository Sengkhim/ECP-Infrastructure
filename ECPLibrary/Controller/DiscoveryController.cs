using ECPLibrary.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECPLibrary.Controller;

[ApiController]
[Route("api/service")]
public class DiscoveryController(IEcpServiceDiscovery discovery) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetService([FromQuery] string serviceName, string endpoint = "")
    {
        var service = await discovery.GetServiceUrlAsync(serviceName, endpoint);

        return Ok(service);
    }
}