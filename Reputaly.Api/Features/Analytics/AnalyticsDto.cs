namespace Reputaly.API.Features.Analytics
{
    public record AnalyticsDto(
        decimal AverageRating,
        int TotalReviews,
        int[] StarDistribution,
        StatusBreakdownDto StatusBreakdown,
        decimal? AverageResponseTimeHours,
        RatingPointDto[] RatingEvolution,
        decimal? AverageSentiment,
        TopicCountDto[] TopTopics
        );

    public record StatusBreakdownDto(
        int Pending,
        int AutoReplied,
        int Replied,
        int Escalated
        );

    public record RatingPointDto(string Label, decimal Value);

    public record TopicCountDto(string  Topic, int Count);
}
