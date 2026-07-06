using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services.Stripe;

namespace Reputaly.API.Features.Billing;

[ApiController]
public class CreatePortalSessionController: ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IStripeService _stripe;
    private readonly IConfiguration _config;

    public CreatePortalSessionController(
        AppDbContext db,
        ITenantContext tenant,
        IStripeService stripe,
        IConfiguration config)
    {
        _db = db;
        _tenant = tenant;
        _stripe = stripe;
        _config = config;
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost("billing/portal")]
    public async Task<IActionResult> Handle()
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == _tenant.TenantId);
        if (tenant is null) return NotFound();

        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
            return BadRequest(new { error = "El tenant no tiene cliente de Stripe todavia" });

        var baseUrl = _config["Frontend:BaseUrl"];
        var url = await _stripe.CreatePortalSessionAsync(
            tenant.StripeCustomerId, returnUrl: $"{baseUrl}/billing");

        return Ok(new { url });
    }
}