using Microsoft.EntityFrameworkCore;
using Reputaly.API.Domain;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;



namespace Reputaly.API.Infrastructure.Multitenancy;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db, ClerkTenantContext tenantContext)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>() == null)
        {
            await _next(context);
            return;
        }

        var orgId = context.User.FindFirst("org_id")?.Value;
        Console.WriteLine($"[TenantMiddleware] org_id del token: {orgId}");

        if (string.IsNullOrEmpty(orgId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "No active organization in token." });
            return;
        }

        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.ClerkOrganizationId == orgId);

        Console.WriteLine($"[TenantMiddleware] Tenant encontrado: {tenant?.Id.ToString() ?? "NULL"}");

        if (tenant == null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                ClerkOrganizationId = orgId,
                Name = orgId,
                CreatedAt = DateTime.UtcNow
            };

            var settings = new TenantSettings
            {
                TenantId = tenant.Id,
                DefaultResponseLanguage = "es",
                AutoDetectLanguage = false,
                AiConfig = new AiConfig()
            };

            db.Tenants.Add(tenant);
            db.TenantSettings.Add(settings);
            await db.SaveChangesAsync();

            Console.WriteLine($"[TenantMiddleware] Tenant creado: {tenant.Id}");
        }

        tenantContext.Resolve(tenant.Id);
        Console.WriteLine($"[TenantMiddleware] TenantId resuelto: {tenant.Id}");

        await _next(context);
    }
}