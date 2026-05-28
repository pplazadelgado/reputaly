namespace Reputaly.API.Features.Settings
{
    public record SettingsDto(
        int AutoReplyMinRate,
        string[] EscalateOnKeyWords,
        int EscalateIfNoReplyHours,
        string AiPersonality,
        string? NotificationEmail
        );

    public record UpdateSettingsRequest(
        int AutoReplyMinRating,
        string[] EscalateOnKeyWords,
        int EscalateIfNoReplyHours,
        string AiPersonality,
        string? NotificationEmail
        );
}
