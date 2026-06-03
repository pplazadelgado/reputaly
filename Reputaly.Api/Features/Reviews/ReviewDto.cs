using System.ComponentModel;

namespace Reputaly.API.Features.Reviews;

public record ReviewDto(
    Guid Id,
    string AuthorName,
    int Rating,
    string? Content,
    DateTime PublishedAt,
    string Status,
    string? AiSuggestedReply,
    string? AiDecision,
    string? AiDecisionReason,
    string? FinalReply,
    DateTime? RepliedAt,
    DateTime? EscalatedAt,
    DateTime CreatedAt,
    Guid LocationId
    );

