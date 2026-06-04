using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Reputaly.API.Features.Tenants;

[ApiController]
public class GetTenantController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public GetTenantController(AppDbContext db, ITenantContext tenant)
    {
        _db= db;    
        _tenant= tenant;
    }

    [Authorize]
    [HttpGet("/tenants/me")]
    public async Task<IActionResult> Handle()
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Id == _tenant.TenantId);

        if (tenant == null) return NotFound();

        return Ok(new TenantDto(tenant.Id, tenant.Name, tenant.Vertical, tenant.SubscriptionPlan, tenant.CreatedAt));
    }
}

