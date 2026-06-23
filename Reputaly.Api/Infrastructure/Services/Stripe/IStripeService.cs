using Stripe;
using Stripe.Checkout;
using Stripe.BillingPortal;

namespace Reputaly.API.Infrastructure.Services.Stripe;

public interface IStripeService
{
    /// <summary>Crea un Customer en Stripe y devuelve su id.</summary>
    Task<string> CreateCustomerAsync(string businessName, Guid tenantId);

    /// <summary>Crea una Checkout Session en modo suscripción. Devuelve la URL.</summary>
    Task<string> CreateCheckoutSessionAsync(
        string customerId, string priceId, string successUrl, string cancelUrl);

    /// <summary>Crea una sesión del Customer Portal. Devuelve la URL.</summary>
    Task<string> CreatePortalSessionAsync(string customerId, string returnUrl);

    /// <summary>Recupera la suscripción activa del customer (o null si no tiene).</summary>
    Task<Subscription> GetActiveSubscriptionAsync(string customerId);

    /// <summary>Actualiza el nombre de un Customer existente en Stripe.</summary>
    Task UpdateCustomerNameAsync(string customerId, string name);
}

