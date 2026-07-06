using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services.Stripe.Billing;
using Reputaly.API.Infrastructure.Services.Stripe;


namespace Reputaly.API.Features.Billing;

[ApiController]
public class CreateCheckoutSessionController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IStripeService _stripe;
    private readonly IPlanResolver _plans;
    private readonly IConfiguration _config;

    public CreateCheckoutSessionController(
        AppDbContext db,
        ITenantContext tenant,
        IStripeService stripe,
        IPlanResolver plans,
        IConfiguration config)
    {
        _db = db;
        _tenant = tenant;
        _stripe = stripe;
        _plans = plans;
        _config = config;
    }

    public record CheckoutRequest(string Plan); // "stater" | "pro"

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost("billing/checkout")]
    public async Task<IActionResult> Handle([FromBody] CheckoutRequest request)
    {
        var priceId = _plans.GetPriceId(request.Plan);
        if (priceId is null)
            return BadRequest(new { error = "Plan no valido o sin precio asociado" });

        //El filtro golbal de Reviews/etc. no aplica a tenants, lo cargamos directo.
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == _tenant.TenantId);
        if (tenant is null) return NotFound();

        // Crear el Customer en Stripe la primera vez(sin email: lo recoge CheckOut)
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            tenant.StripeCustomerId = await _stripe.CreateCustomerAsync(tenant.Name, tenant.Id);
            await _db.SaveChangesAsync();
        }

        var baseUrl = _config["Frontend:BaseUrl"];
        // TEMPORAL — depuración
        var url = await _stripe.CreateCheckoutSessionAsync(
            tenant.StripeCustomerId,
            priceId,
            successUrl: $"{baseUrl}/billing?checkout=success",
            cancelUrl: $"{baseUrl}/billing?checkout=cancel");

        return Ok(new { url });
    }
}

