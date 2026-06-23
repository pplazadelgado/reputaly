using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services.Stripe;

namespace Reputaly.API.Features.Tenants;

[ApiController]
public class UpdateTenatController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IStripeService _stripe;
    private readonly ILogger<UpdateTenatController> _logger;

    public UpdateTenatController(
        AppDbContext db,
        ITenantContext tenant,
        IStripeService stripe,
        ILogger<UpdateTenatController> logger)
    {
        _db = db;
        _tenant = tenant;
        _stripe = stripe;
        _logger = logger;
    }

    [Authorize]
    [HttpPut("/tenants/me")]
    public async Task<IActionResult> Handle([FromBody] UpdateTenantRequest request)
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Id == _tenant.TenantId);

        if (tenant == null) return NotFound();

        tenant.Name = request.Name;
        tenant.Vertical = request.Vertical;
        await _db.SaveChangesAsync();

        // Si el tenant ya tiene Customer en Stripe, sincronizamos el nombre.
        // Best-effort: un fallo de Stripe no debe romper el guardado del tenant.
        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            try
            {
                await _stripe.UpdateCustomerNameAsync(tenant.StripeCustomerId, tenant.Name);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,
                    "No se pudo actualizar el nombre del Customer en Stripe (tenant {TenantId}",
                    tenant.Id);
            }
        }

        return Ok(new TenantDto(tenant.Id, tenant.Name, tenant.Vertical, tenant.SubscriptionPlan, tenant.CreatedAt));
    }
}

