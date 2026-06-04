using Reputaly.API.Domain;

namespace Reputaly.API.Features.Settings;

public record SettingsDto(
    int AutoReplyMinRating,
    string[] EscalateOnKeywords,
    int EscalateIfNoReplyHours,
    AiConfig AiConfig,
    string DefaultResponseLanguage,
    bool AutoDetectLanguage,
    string? NotificationEmail
);

public record UpdateSettingsRequest(
    int AutoReplyMinRating,
    string[] EscalateOnKeywords,
    int EscalateIfNoReplyHours,
    AiConfig AiConfig,
    string DefaultResponseLanguage,
    bool AutoDetectLanguage,
    string? NotificationEmail
);
