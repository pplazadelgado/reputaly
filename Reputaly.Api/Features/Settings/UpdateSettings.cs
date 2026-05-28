using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;


namespace Reputaly.API.Features.Settings;

[ApiController]
public class UpdateSettingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public UpdateSettingsController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [Authorize]
    [HttpPut("/tenants/me/settings")]
    public async Task<IActionResult> Handle([FromBody] UpdateSettingsRequest request)
    {
        var s = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == _tenant.TenantId);

        if (s == null) return NotFound();

        s.AutoReplyMinRating = request.AutoReplyMinRating;
        s.EscalateOnKeywords = request.EscalateOnKeyWords;
        s.EscalateIfNoReplyHours = request.EscalateIfNoReplyHours;
        s.AiPersonality = request.AiPersonality;
        s.NotificationEmail = request.NotificationEmail;

        await _db.SaveChangesAsync();

        return Ok(new SettingsDto(
            s.AutoReplyMinRating,
            s.EscalateOnKeywords,
            s.EscalateIfNoReplyHours,
            s.AiPersonality,
            s.NotificationEmail
            ));
    }
}

