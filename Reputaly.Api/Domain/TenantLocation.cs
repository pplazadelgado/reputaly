namespace Reputaly.API.Domain;

public class TenantLocation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GoogleLocationId { get; set; }
    public string? GoogleAccessToken { get; set; }
    public string? GoogleRefreshToken { get; set; }
    public DateTime? GoogleTokenExpiresAt { get; set; }  // typo corregido
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Idioma de respuesta específico para esta ubicación
    public string? ResponseLanguage { get; set; }

    public string? GoogleAccountEmail {  get; set; }

    public Tenant Tenant { get; set; } = null!;
}