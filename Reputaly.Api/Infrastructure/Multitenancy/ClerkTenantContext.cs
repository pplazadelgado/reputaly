using Microsoft.AspNetCore.Mvc.Rendering;

namespace Reputaly.API.Infrastructure.Multitenancy
{
    public class ClerkTenantContext : ITenantContext
    {
        public Guid TenantId { get; private set; }
        public bool IsResolved {  get; private set; }

        public void Resolve (Guid tenantId)
        {
            TenantId = tenantId;
            IsResolved = true;
        }
    }
}
