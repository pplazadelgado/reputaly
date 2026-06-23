using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Persistence;
using Reputaly.API.Infrastructure.Services.Billing;
using Reputaly.API.Infrastructure.Services.Email;

namespace Reputaly.API.Infrastructure.Services.Background;

public class EscalationTimeoutProcessor : IEscalationTimeoutProcessor
{
    private readonly AppDbContext _db;
    private readonly ISubscriptionLimitsService _limits;
    private readonly IEmailService _email;
    private readonly ILogger<EscalationTimeoutProcessor> _logger;

    public EscalationTimeoutProcessor(
        AppDbContext db,
        ISubscriptionLimitsService limits,
        IEmailService email,
        ILogger<EscalationTimeoutProcessor> logger)
    {
        _db = db;
        _limits = limits;
        _email = email;
        _logger = logger;
    }

    public async Task<int> ProcessTimeoutsAsync(CancellationToken ct = default)
    {
        var candidates = await _db.Reviews
            .IgnoreQueryFilters()
            .Where(r => r.Status == "escalated"
                     && r.AiSuggestedReply != null
                     && r.EscalatedAt != null)
            .ToListAsync(ct);

        if (candidates.Count == 0) return 0;

        var tenantIds = candidates.Select(r => r.TenantId).Distinct().ToList();
        var settingsByTenant = await _db.TenantSettings
            .IgnoreQueryFilters()
            .Where(s => tenantIds.Contains(s.TenantId))
            .ToDictionaryAsync(s => s.TenantId, ct);

        var now = DateTime.UtcNow;
        int published = 0;

        foreach (var review in candidates)
        {
            if (ct.IsCancellationRequested) break;

            if (!settingsByTenant.TryGetValue(review.TenantId, out var settings))
                continue;

            var deadline = review.EscalatedAt!.Value.AddHours(settings.EscalateIfNoReplyHours);
            if (now < deadline) continue;

            if (!await _limits.CanAutoReplyAsync(review.TenantId)) continue;

            // Releemos por si un humano la respondió mientras tanto.
            var fresh = await _db.Reviews
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == review.Id, ct);

            if (fresh is null || fresh.Status != "escalated") continue;

            fresh.FinalReply = fresh.AiSuggestedReply;
            fresh.Status = "auto_replied";
            fresh.RepliedAt = now;
            fresh.AutoReplied = true;

            await _db.SaveChangesAsync(ct);

            await _email.SendEscalationEmailAsync(
                fresh.TenantId, fresh.Id, autoRepliedByTimeout: true);

            published++;
        }

        if (published > 0)
            _logger.LogInformation(
                "Timeout: {Count} reseñas auto-respondidas", published);

        return published;
    }
}
