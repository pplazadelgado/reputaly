using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;

namespace Reputaly.API.Features.Tenants;

[ApiController]
public class UpdateTenatController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public UpdateTenatController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [Authorize]
    [HttpPut("/tenants/me")]
    public async Task<IActionResult> Handle([FromBody] UpdateTenantRequest request)
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Id == _tenant.TenantId);

        if (tenant == null) return NotFound();

        tenant.Name = request.name;
        await _db.SaveChangesAsync();

        return Ok(new TenantDto(tenant.Id, tenant.Name, tenant.SubscriptionPlan, tenant.CreatedAt));
    }
}

