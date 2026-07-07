using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services.Stripe.Billing;
using Reputaly.API.Infrastructure.Services.Stripe;

namespace Reputaly.API.Features.Billing;

[ApiController]
public class GetBillingStatusController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IStripeService _stripe;
    private readonly IPlanResolver _plans;

    public GetBillingStatusController(
        AppDbContext db,
        ITenantContext tenant,
        IStripeService stripe,
        IPlanResolver plans)
    {
        _db = db;
        _tenant = tenant;
        _stripe = stripe;
        _plans = plans;
    }

    public record BillingStatusDto(
        string Plan,
        int MaxLocations,
        int MonthlyAiReplies,      // -1 = ilimitado
        int AiRepliesUsed,
        bool CanAutoReply,
        DateTime? CurrentPeriodEnd,
        bool CancelAtPeriodEnd);

    [Authorize(Policy = "RequireAdmin")]
    [HttpGet("billing/status")]
    public async Task<IActionResult> Handle()
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == _tenant.TenantId);
        if (tenant is null) return NotFound();

        var planDef = _plans.GetPlanDefinition(tenant.SubscriptionPlan)
            ?? _plans.GetPlanDefinition("free");

        // Uso del mes en curso: Reviews con respuesta IA generada este mes
        var startOfMonth = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1,0,0,0, DateTimeKind.Utc);

        var aiRepliesUsed = await _db.Reviews
            .Where(r => r.AiSuggestedReply != null && r.CreatedAt >= startOfMonth)
            .CountAsync();

        //Datos en vivo de Stripe(fecha de renovacion, cancelacion pendiente)
        DateTime? periodEnd = null;
        bool cancelAtPeriodEnd = false;
        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            var sub = await _stripe.GetActiveSubscriptionAsync(tenant.StripeCustomerId);
            if (sub?.Items?.Data is not null)
            {
                periodEnd = sub.Items.Data.FirstOrDefault()?.CurrentPeriodEnd;
                cancelAtPeriodEnd = sub.CancelAtPeriodEnd;
            }
        }

        return Ok(new BillingStatusDto(
            planDef.Plan,
            planDef.MaxLocations,
            planDef.MonthlyAiReplies,
            aiRepliesUsed,
            planDef.CanAutoReply,
            periodEnd,
            cancelAtPeriodEnd));
    }


}


