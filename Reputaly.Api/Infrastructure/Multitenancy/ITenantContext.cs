namespace Reputaly.API.Infrastructure.Multitenancy
{
    public interface ITenantContext
    {
        Guid TenantId {  get; }
        bool IsResolved {  get; }
    }
}
