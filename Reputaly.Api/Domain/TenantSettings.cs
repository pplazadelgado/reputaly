namespace Reputaly.API.Domain
{
    public class TenantSettings
    {
        // TenantId es a la vez PF y FK - relacion 1:1 con Tenant
        public Guid TenantId {  get; set; }
        public int AutoReplyMinRating { get; set; } = 4;
        public string[] EscalateOnKeywords {  get; set; } = Array.Empty<string>();
        public int EscalateIfNoReplyHours { get; set; } = 24;
        public string AiPersonality {  get; set; }=string.Empty;
        public string? NotificationEmail {  get; set; }

        public Tenant Tenant { get; set; } = null;
    }
}
