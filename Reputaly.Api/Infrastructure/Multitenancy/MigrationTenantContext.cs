namespace Reputaly.API.Infrastructure.Multitenancy
{
    public class MigrationTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
        public bool IsResolved => false;
    }
}
