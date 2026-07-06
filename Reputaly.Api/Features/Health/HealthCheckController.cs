using Microsoft.AspNetCore.Mvc;

namespace Reputaly.API.Features.Health;

[ApiController]
public class HealthCheckController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult Handle() => Ok(new { status = "ok" });
}