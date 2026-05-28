using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Reputaly.API.Infrastructure.Multitenancy;


namespace Reputaly.API.Infrastructure.Persistence;

// Esta clase solo la usa el comando "dotnet ef". Nunca se ejecuta en producción.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=ep-delicate-voice-aln65afq-pooler.c-3.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_em1MzLT4dfqs;Ssl Mode=Require")
            .Options;

        return new AppDbContext(options, new MigrationTenantContext());
    }
}
