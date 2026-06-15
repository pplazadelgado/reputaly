using Microsoft.Extensions.Options;
using Reputaly.API.Configuration;

namespace Reputaly.API.Infrastructure.Services.Stripe.Billing
{
    public class PlanResolver :IPlanResolver
    {
        private readonly StripeOptions _options;

        public PlanResolver(IOptions<StripeOptions> options)
        {
            _options = options.Value;
        }

        public string? ResolvePlanFromPriceId(string priceId)
            => _options.Plans
            .FirstOrDefault(p => p.PriceId == priceId && !string.IsNullOrEmpty(priceId))
            ?.Plan;

        public SubscriptionPlan? GetPlanDefinition(string plan)
            => _options.Plans.FirstOrDefault(p => p.Plan == plan);

        public string? GetPriceId(string plan)
        {
            var id = _options.Plans.FirstOrDefault(p => p.Plan == plan)?.PriceId;
            return string.IsNullOrEmpty(id) ? null : id;
        }
    }
}
