namespace Reputaly.API.Features.Tenants
{
    public record TenantDto(
        Guid Id,
        string Name,
        string SubscriptionPlan,
        DateTime CreatedAt
        );

    public record UpdateTenantRequest(string name);
  
}
