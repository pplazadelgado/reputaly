using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services.Stripe.Billing;
using Stripe;

namespace Reputaly.API.Infrastructure.Services.Billing;

public class SubscriptionLimitsService : ISubscriptionLimitsService
{
    private readonly AppDbContext _db;
    private readonly IPlanResolver _plans;

    public SubscriptionLimitsService(AppDbContext db, IPlanResolver plans)
    {
        _db = db;
        _plans = plans;
    }

    public async Task<bool> CanAutoReplyAsync(Guid tenantId)
    {
        var plan = await GetPlanDefinitionAsync(tenantId);
        return plan?.CanAutoReply ?? false;
    }

    public async Task<bool>CanAddLocationAsync(Guid tenantId)
    {
        var plan = await GetPlanDefinitionAsync(tenantId);
        if (plan is null) return false;

        // -1 = ilimitado (Pro)
        if (plan.MaxLocations == -1) return true;

        var activeLocations = await _db.TenantLocations
            .IgnoreQueryFilters()
            .CountAsync(l => l.TenantId == tenantId && l.IsActive);

        return activeLocations < plan.MaxLocations;
    }

    public async Task<int> AiRepliesRemainingAsync(Guid tenantId)
    {
        var plan = await GetPlanDefinitionAsync(tenantId);
        if (plan is null) return 0;

        // -1 = ilimitado(Pro)
        if (plan.MonthlyAiReplies == -1) return -1;

        // 0 en config = plan si IA (Free)
        if (plan.MonthlyAiReplies == 0) return 0;

        var used = await AiRepliesUsedThisMonthAsync(tenantId);
        var remaining = plan.MonthlyAiReplies - used;
        return remaining < 0 ? 0 : remaining;
    }

    // -------------------------------------------------------
    // Helpers privados
    // -------------------------------------------------------

    private async Task<Configuration.SubscriptionPlan?> GetPlanDefinitionAsync(Guid tenantId)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant is null) return null;

        return _plans.GetPlanDefinition(tenant.SubscriptionPlan)
            ?? _plans.GetPlanDefinition("free");
    }

    private async Task<int> AiRepliesUsedThisMonthAsync(Guid tenantId)
    {
        var startOfMonth = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1,0,0,0, DateTimeKind.Utc);

        return await _db.Reviews
            .IgnoreQueryFilters()
            .CountAsync(r => r.TenantId == tenantId
                && r.AiSuggestedReply != null
                && r.CreatedAt >= startOfMonth);
    }
}
