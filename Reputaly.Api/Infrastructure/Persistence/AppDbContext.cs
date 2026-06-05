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
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relación 1:1 Tenant ↔ TenantSettings
        modelBuilder.Entity<TenantSettings>()
            .HasKey(s => s.TenantId);

        modelBuilder.Entity<Tenant>()
            .HasOne(t => t.Settings)
            .WithOne(s => s.Tenant)
            .HasForeignKey<TenantSettings>(s => s.TenantId);

        // Array de strings en PostgreSQL
        modelBuilder.Entity<TenantSettings>()
            .Property(s => s.EscalateOnKeywords)
            .HasColumnType("text[]");

        // -------------------------------------------------------
        // Configuración de Review
        // -------------------------------------------------------
        modelBuilder.Entity<Review>(entity =>
        {
            // Índice único: evita duplicar la misma reseña de Google por ubicación
            entity.HasIndex(e => new { e.LocationId, e.GoogleReviewId })
                  .IsUnique();

            // Índice para las queries del panel (tenant + ubicación + estado)
            entity.HasIndex(e => new { e.TenantId, e.LocationId, e.Status });

            // Topics como array nativo de PostgreSQL
            entity.Property(e => e.DetectedTopics)
                  .HasColumnType("text[]");

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Location)
                  .WithMany()
                  .HasForeignKey(e => e.LocationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Filtro global: todas las queries de Reviews se filtran por tenant
        modelBuilder.Entity<Review>()
            .HasQueryFilter(r => r.TenantId == _tenantContext.TenantId);

        // AiConfig como jsonb — ValueConverter para serializar/deserializar
        var aiConfigConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<AiConfig, string>(
            v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
            v => System.Text.Json.JsonSerializer.Deserialize<AiConfig>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new AiConfig()
        );

        modelBuilder.Entity<TenantSettings>()
            .Property(s => s.AiConfig)
            .HasColumnType("jsonb")
            .HasConversion(aiConfigConverter);

        // Filtros globales por TenantId
        modelBuilder.Entity<TenantUser>()
            .HasQueryFilter(u => u.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<TenantLocation>()
            .HasQueryFilter(l => l.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<TenantSettings>()
            .HasQueryFilter(s => s.TenantId == _tenantContext.TenantId);
    }
}
