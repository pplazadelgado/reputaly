using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Multitenancy;
using Reputaly.API.Infrastructure.Persistence;

namespace Reputaly.API.Features.Analytics;

[ApiController]
public class GetAnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public GetAnalyticsController(AppDbContext db, ITenantContext tenant)
    {
        _db = db; 
        _tenant = tenant;
    }

    [Authorize]
    [HttpGet("/analytics")]
    public async Task<IActionResult> Handle(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? locationId)
    {
        // Defaul: ultimos 30 dias
        var toUtc = (to ?? DateTime.UtcNow).ToUniversalTime();
        var fromUtc = (from ?? toUtc.AddDays(-30)).ToUniversalTime();

        //Query base reutilizable. El filtro global por TenantId se aplica solo.
        IQueryable<Domain.Review> Base() => _db.Reviews
            .Where(r => r.PublishedAt >= fromUtc && r.PublishedAt <= toUtc)
            .Where(r => locationId == null || r.LocationId == locationId);

        var totalReviews = await Base().CountAsync();

        if(totalReviews == 0)
        {
            return Ok(new AnalyticsDto(
                AverageRating: 0,
                TotalReviews: 0,
                StarDistribution: new[] { 0, 0, 0, 0, 0 },
                StatusBreakdown: new StatusBreakdownDto(0, 0, 0, 0),
                AverageResponseTimeHours: null,
                RatingEvolution: Array.Empty<RatingPointDto>(),
                AverageSentiment: null,
                TopTopics: Array.Empty<TopicCountDto>()
            ));
        }

        // Rating medio
        var averageRating = (decimal)await Base().AverageAsync(r => (double)r.Rating);

        // Distribucion por estres (1..5)
        var byStar = await Base()
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync();

        var startDistribution = new int[5];
        foreach( var row in byStar)
        {
            if (row.Rating >= 1 && row.Rating <= 5)
                startDistribution[row.Rating - 1] = row.Count;
        }

        // Desglose por estado
        var byStatus = await Base()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int CountFor(string s) => byStatus.FirstOrDefault(x => x.Status == s)?.Count ?? 0;
        var statusBreakdown = new StatusBreakdownDto(
            Pending: CountFor("pending"),
            AutoReplied: CountFor("auto_replied"),
            Replied: CountFor("replied"),
            Escalated: CountFor("escalated")
            );

        // Tiempo medio de respuesta (en horas).
        // Restar dos DateTime se traduce en Postgres a una resta nativa
        // que devuelve un intervalo; pedimos TotalHours en cliente.
        var responseSpans = await Base()
            .Where(r => r.RepliedAt != null)
            .Select(r => r.RepliedAt!.Value - r.PublishedAt)
            .ToListAsync();

        decimal? averageResponseTimeHours = responseSpans.Count > 0
            ? Math.Round((decimal)responseSpans.Average(ts => ts.TotalHours), 2)
            : null;

        // Evolución del rating por semana.
        // Agrupamos por año + número de semana ISO (ambos enteros, soportados por Npgsql vía EXTRACT).
        var weekly = await Base()
            .GroupBy(r => new { Year = r.PublishedAt.Year, Week = r.PublishedAt.DayOfYear / 7 })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Week,
                AnyDate = g.Min(r => r.PublishedAt),
                AvgRating = g.Average(r => (double)r.Rating)
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Week)
            .ToListAsync();

        var ratingEvolution = weekly
            .Select(w => new RatingPointDto(
                Label: w.AnyDate.ToString("dd MMM"),
                Value: Math.Round((decimal)w.AvgRating, 2)
            ))
            .ToArray();

        // Sentiment medio (solo reseñas con score)
        decimal? averageSentiment = null;
        var sentimentValues = await Base()
            .Where(r => r.SentimentScore != null)
            .Select(r => r.SentimentScore!.Value)
            .ToListAsync();

        // Top topics - traemos solo la columna del array, agregamos en memoria
        var topicArrays = await Base()
            .Where(r => r.DetectedTopics != null)
            .Select(r => r.DetectedTopics!)
            .ToListAsync();

        var topTopics = topicArrays
            .SelectMany(arr => arr)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .GroupBy(t => t)
            .Select(g => new TopicCountDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToArray();

        return Ok(new AnalyticsDto(
            AverageRating: Math.Round(averageRating, 2),
            TotalReviews: totalReviews,
            StarDistribution: startDistribution,
            StatusBreakdown: statusBreakdown,
            AverageResponseTimeHours: averageResponseTimeHours,
            RatingEvolution: ratingEvolution,
            AverageSentiment: averageSentiment,
            TopTopics: topTopics
            ));
    }
}

