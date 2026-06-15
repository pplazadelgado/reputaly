using Stripe;
using Stripe.Checkout;
using Stripe.BillingPortal;

namespace Reputaly.API.Infrastructure.Services.Stripe
{
    public class StripeService :IStripeService
    {
        public async Task<string> CreateCustomerAsync(string businessName, Guid tenantId)
        {
            var options = new CustomerCreateOptions
            {
                Name = businessName,
                Metadata = new Dictionary<string, string>
                {
                    ["tenant_id"] = tenantId.ToString()
                }
            };
            var service = new CustomerService();
            var customer = await service.CreateAsync(options);
            return customer.Id;
        }

        public async Task<string> CreateCheckoutSessionAsync(
            string customerId, string priceId, string successUrl, string cancelUrl)
        {
            var options = new global::Stripe.Checkout.SessionCreateOptions
            {
                Customer = customerId,
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
            {
                new() { Price = priceId, Quantity = 1 }
            },
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };
            var service = new global::Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }

        public async Task<string> CreatePortalSessionAsync(string customerId, string returUrl)
        {
            var options = new global::Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = returUrl
            };
            var service = new global::Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }

        public async Task<Subscription> GetActiveSubscriptionAsync(string customerId)
        {
            var service = new SubscriptionService();
            var list = await service.ListAsync(new SubscriptionListOptions
            {
                Customer = customerId,
                Status = "all",
                Limit = 1
            });

            return list.Data.FirstOrDefault();

        }
    }
}
