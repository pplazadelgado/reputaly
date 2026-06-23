using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reputaly.API.Infrastructure;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Services.Email;

namespace Reputaly.API.Features.Dev;

[ApiController]
public class RunWeeklySummaryController : ControllerBase
{
    private readonly IEmailService _email;
    private readonly ITenantContext _tenant;

    public RunWeeklySummaryController(IEmailService email, ITenantContext tenant)
    {
        _email = email;
        _tenant = tenant;
    }

    // SOLO DESARROLLO — dispara el resumen semanal del tenant actual a demanda.
    [Authorize]
    [DevelopmentOnly]
    [HttpPost("/dev/run-weekly-summary")]
    public async Task<IActionResult> Handle()
    {
        await _email.SendWeeklySummaryAsync(_tenant.TenantId);
        return Ok(new { message = "Resumen semanal disparado. Revisa tu bandeja y los logs." });
    }
}
