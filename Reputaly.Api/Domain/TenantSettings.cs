namespace Reputaly.API.Domain;

public class TenantSettings
{
    public Guid TenantId { get; set; }
    public int AutoReplyMinRating { get; set; } = 4;
    public string[] EscalateOnKeywords { get; set; } = Array.Empty<string>();
    public int EscalateIfNoReplyHours { get; set; } = 24;

    // AiConfig reemplaza AiPersonality — se guarda como jsonb
    public AiConfig AiConfig { get; set; } = new AiConfig();

    // Idioma de respuesta
    public string DefaultResponseLanguage { get; set; } = "es";
    public bool AutoDetectLanguage { get; set; } = false;

    public string? NotificationEmail { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
