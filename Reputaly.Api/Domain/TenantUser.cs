using Microsoft.AspNetCore.Identity;

namespace Reputaly.API.Domain
{
    public class TenantUser
    {
        public Guid Id {  get; set; }
        public Guid TenantId {  get; set; }
        public string ClerkUserId {  get; set; } = string.Empty;
        public string Role { get; set; } = "member"; //admin | member
        public DateTime InvitedAt {  get; set; } = DateTime.UtcNow;
        public DateTime? JoinedAt {  get; set; }

        public Tenant Tenant { get; set; }
    }
}
