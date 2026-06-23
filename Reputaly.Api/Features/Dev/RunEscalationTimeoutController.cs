using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reputaly.API.Infrastructure;
using Reputaly.API.Infrastructure.Services.Background;

namespace Reputaly.API.Features.Dev;

[ApiController]
public class RunEscalationTimeoutController : ControllerBase
{
    private readonly IEscalationTimeoutProcessor _processor;

    public RunEscalationTimeoutController(IEscalationTimeoutProcessor processor)
    {
        _processor = processor;
    }

    // SOLO DESARROLLO — dispara la revisión de timeouts a demanda.
    [Authorize]
    [DevelopmentOnly]
    [HttpPost("/dev/run-escalation-timeout")]
    public async Task<IActionResult> Handle()
    {
        var count = await _processor.ProcessTimeoutsAsync();
        return Ok(new { message = $"Revisión completada. {count} reseñas auto-respondidas." });
    }
}