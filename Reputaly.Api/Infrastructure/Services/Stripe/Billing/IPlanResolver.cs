using Reputaly.API.Configuration;

namespace Reputaly.API.Infrastructure.Services.Stripe.Billing
{
    public interface IPlanResolver
    {
        /// <summary>Plan string ("starter"/"pro"...) a partir de un priceId de Stripe. null si no mapea.</summary>
        string? ResolvePlanFromPriceId(string priceId);

        /// <summary>Definición completa de un plan por su string. null si no existe.</summary>
        SubscriptionPlan? GetPlanDefinition(string plan);

        /// <summary>El priceId de un plan dado. null si es free o no existe.</summary>
        string? GetPriceId(string plan);
    }
}
