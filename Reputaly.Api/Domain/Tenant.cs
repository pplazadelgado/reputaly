namespace Reputaly.API.Domain
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string ClerkOrganizationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? StripeCustomerId {  get; set; }
        public string SubscriptionPlan { get; set; } = "free";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navegacion - EF usa esto para hacer joins automaticamente
        public TenantSettings? Settings { get; set; }
        public ICollection<TenantUser> Users = new List<TenantUser>();
        public ICollection<TenantLocation> Locations { get; set; } = new List<TenantLocation>();
    }
}
