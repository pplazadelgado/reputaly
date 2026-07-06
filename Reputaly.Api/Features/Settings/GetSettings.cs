using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;

namespace Reputaly.API.Features.Settings;

[ApiController]
public class GetSettingsController: ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public GetSettingsController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpGet("/tenants/me/settings")]
    public async Task<IActionResult> Handle()
    {
        var s = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == _tenant.TenantId);

        if (s == null) return NotFound();

        return Ok(new SettingsDto(
            s.AutoReplyMinRating,
            s.EscalateOnKeywords,
            s.EscalateIfNoReplyHours,
            s.AiConfig,
            s.DefaultResponseLanguage,
            s.AutoDetectLanguage,
            s.NotificationEmail
        ));
    }
}

