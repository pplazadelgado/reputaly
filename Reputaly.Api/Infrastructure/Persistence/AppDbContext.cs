using Microsoft.EntityFrameworkCore;
using Reputaly.API.Domain;
using Reputaly.API.Infrastructure.Multitenancy;

namespace Reputaly.API.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<TenantLocation> TenantLocations => Set<TenantLocation>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relación 1:1 Tenant ↔ TenantSettings
        modelBuilder.Entity<TenantSettings>()
            .HasKey(s => s.TenantId);

        modelBuilder.Entity<Tenant>()
            .HasOne(t => t.Settings)
            .WithOne(s => s.Tenant)
            .HasForeignKey<TenantSettings>(s => s.TenantId);

        // Array de strings en PostgreSQL (tipo text[])
        modelBuilder.Entity<TenantSettings>()
            .Property(s => s.EscalateOnKeywords)
            .HasColumnType("text[]");

        // Filtros globales — se aplican solos en cada consulta
        // Nota: Tenant NO tiene filtro (es la propia tabla raíz)
        modelBuilder.Entity<TenantUser>()
            .HasQueryFilter(u => u.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<TenantLocation>()
            .HasQueryFilter(l => l.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<TenantSettings>()
            .HasQueryFilter(s => s.TenantId == _tenantContext.TenantId);
    }
}
